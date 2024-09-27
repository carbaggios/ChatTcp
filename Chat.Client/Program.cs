using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Unicode;

namespace Chat.Client
{
    internal class Program
    {
        private const int _serverPort = 9901;
        static async Task Main(string[] args)
        {
            var udpClient = new UdpClient(/*9910*/);
            await udpClient.SendAsync(Encoding.UTF8.GetBytes("Hello I'm a new client"), new IPEndPoint(IPAddress.Broadcast, 9910));

            Task.Run(async () =>
            {
                do
                {
                    var result = await udpClient.ReceiveAsync();
                    Console.WriteLine($"{Encoding.UTF8.GetString(result.Buffer)} -> {result.RemoteEndPoint}");
                }
                while (true);
            });

            using var tcpClient = new TcpClient();
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
                    Console.WriteLine(message);
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
        }
    }
}
