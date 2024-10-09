namespace Chat.Server
{
    class ChatMessageEventArgs : EventArgs
    {
        public string? Message { get; }

        public ChatMessageEventArgs(string? message)
        {
            Message = message;
        }
    }
}
