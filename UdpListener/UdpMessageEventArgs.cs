namespace Udp
{
    public class UdpMessageEventArgs : EventArgs
    {
        public string? Message { get; }

        public UdpMessageEventArgs(string? message)
        {
            Message = message;
        }
    }
}
