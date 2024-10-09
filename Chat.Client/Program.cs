using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Unicode;
using Udp;

namespace Chat.Client
{
    internal class Program
    {
        private const int _serverPort = 9001;
        private static UdpClient udpClient = new UdpClient();
        private static IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 8001);
        static async Task Main(string[] args)
        {
            
            //var udpClient = new UdpClient();
            //IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 8001);
            await udpClient.SendAsync(Encoding.UTF8.GetBytes("Ping"), endPoint);

            Console.WriteLine("Connected");
            
            var udpListener = new Listener();
            udpListener.MessageReceived += Udp_MessageReceived;
            udpListener.Start(udpClient);

            using TcpClient tcpClient = await ListenTcp();
        }

        private static void Udp_MessageReceived(object? sender, UdpMessageEventArgs e)
        {
            //var endPoint = sender as IPEndPoint;
            //var udpClient = new UdpClient();

            Console.WriteLine("Client Udp_MessageReceived: Message=" + e.Message);

            if (e.Message == "Server Login")
            {
                Task.Run(() =>
                {
                    udpClient.SendAsync(Encoding.UTF8.GetBytes("Ping2"), endPoint);
                });
            }
        }

        private static async Task<TcpClient> ListenTcp()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(System.Net.IPAddress.Loopback, _serverPort);

            var stream = tcpClient.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);

            var readerTask = Task.Run(() =>
            {
                string? message = null;
                do
                {
                    
                    message = reader.ReadLine();
                    //Console.WriteLine("ProtocolType=" + stream.Socket.ProtocolType.ToString());
                    Console.WriteLine("Client ListenTcp=" + message);
                }
                while (true);
            });

            var writerTask = Task.Run(() =>
            {
                do
                {
                    var message = Console.ReadLine();
                    writer.WriteLine(message);
                    writer.Flush();
                }
                while (true);
            });

            await Task.WhenAll(readerTask, writerTask);
            return tcpClient;
        }
    }
}
