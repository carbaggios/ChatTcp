using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using Udp;

namespace Chat.Server
{
    internal class Program
    {
        private const int _tcpPort = 9001;
        private static List<ChatClient> _clients = new List<ChatClient>();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting UDP Listener");
            var udpListener = new Listener();
            udpListener.MessageReceived += Udp_MessageReceived;
            udpListener.Start();

            Console.WriteLine("Starting TCP Listener");
            var server = new TcpListener(System.Net.IPAddress.Any, _tcpPort);
            server.Start();

            Console.WriteLine("Server started");

            await ListenTcp(server);
        }

        private static async Task ListenTcp(TcpListener server)
        {
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

        private static void Udp_MessageReceived(object? sender, UdpMessageEventArgs e)
        {
            var endPoint = sender as IPEndPoint;
            var udpClient = new UdpClient();

            if (e.Message == "Ping")
            {
                Task.Run(() =>
                {
                    udpClient.SendAsync(Encoding.UTF8.GetBytes("Server Login"), endPoint);
                });
            }

            if (e.Message == "Ping2")
            {
                Task.Run(() =>
                {
                    udpClient.SendAsync(Encoding.UTF8.GetBytes("Server Password"), endPoint);
                    //udpClient.SendAsync(Encoding.UTF8.GetBytes($"Server Udp_MessageReceived_2: {endPoint.Address}:{endPoint.Port}"), endPoint);
                });
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
