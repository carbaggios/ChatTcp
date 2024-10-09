using System.Net.Sockets;
using System.Text;

namespace Udp;

public class Listener
{
    private const int _defaultPort = 8001;
    public int Port { get; init; /*private set;*/ } = _defaultPort;

    public Listener() { }

    public Listener(int port)
    {
        Port = port;
    }

    public void Start()
    {
        var udpClient = new UdpClient(Port);
        Start(udpClient);
    }

    public void Start(UdpClient udpClient)
    {
        Task task = Task.Run(async () =>
        {
            do
            {
                var result = await udpClient.ReceiveAsync();
                //Console.WriteLine($"{Encoding.UTF8.GetString(result.Buffer)} -> {result.RemoteEndPoint}");
                Console.WriteLine($"Service message -> {result.RemoteEndPoint}");
                MessageReceived?.Invoke(result.RemoteEndPoint, new UdpMessageEventArgs(Encoding.UTF8.GetString(result.Buffer)));
            }
            while (true);
        });
    }

    public event EventHandler<UdpMessageEventArgs>? MessageReceived;
}

