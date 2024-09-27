using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Chat.Server
{
    class ChatClient:IDisposable
    { 
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public TcpClient Client => _tcpClient;
        public ChatClient(TcpClient tcpClient)
        { 
            _tcpClient = tcpClient;

            _stream = _tcpClient.GetStream();
            _reader = new StreamReader(_stream);
            _writer = new StreamWriter(_stream);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public Task Run()
        {
            return Task.Run( () => 
            { 
                string? message = null;
                do
                {
                    message = _reader.ReadLine();
                    Log(message);
                    MessageReceived?.Invoke(this, new ChatMessageEventArgs(message));
                }
                while (!string.IsNullOrEmpty(message));
            });
        }

        public async Task SendMessage(string? message)
        { 
            await _writer.WriteLineAsync(message);
            await _writer.FlushAsync();
        }

        private void Log(string? message)
        { 
            Console.WriteLine($"[{_tcpClient.Client.RemoteEndPoint}]: {message}");
        }

        public event EventHandler<ChatMessageEventArgs>? MessageReceived;
    }

    class ChatMessageEventArgs : EventArgs
    {
        public string? Message { get; }

        public ChatMessageEventArgs(string? message)
        {
            Message = message;
        }
    }

    internal class Program
    {
        private const int _port = 9901;
        private static List<ChatClient> _clients = new List<ChatClient>();
        static async Task Main(string[] args)
        {
            var udpClient = new UdpClient(9910);
            Task.Run(async () =>
            {
                do
                {
                    var result = await udpClient.ReceiveAsync();
                    Console.WriteLine($"{Encoding.UTF8.GetString(result.Buffer)} -> {result.RemoteEndPoint}");
                }
                while (true);
            });

            var server = new TcpListener(System.Net.IPAddress.Any, _port);
            server.Start();

            try
            {
                while (true)
                {
                    var tcpClient = await server.AcceptTcpClientAsync();
                    var client = new ChatClient(tcpClient);
                    client.MessageReceived += Client_MessageReceived;

                    _clients.Add(client);
                    var task = client.Run();
                }
            }
            finally
            { 
                server.Stop();
            }
        }

        private static void Client_MessageReceived(object? sender, ChatMessageEventArgs e)
        {
            var client = sender as ChatClient;
            if (client == null)
                return;

            var message = $"[{client.Client.Client.RemoteEndPoint}]: {e.Message}";

            //var sended = new List<Task>(_clients.Count);
            foreach (var item in _clients)
            {
                //sended.Add(item.SendMessage(message));
                var t = item.SendMessage(message);
            }
        }
    }
}
