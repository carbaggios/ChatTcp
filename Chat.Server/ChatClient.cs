using System.Net.Sockets;

namespace Chat.Server
{
    class ChatClient :IDisposable
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
}
