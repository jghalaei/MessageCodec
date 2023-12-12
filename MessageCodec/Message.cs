namespace MessageCodec
{
    public class Message
    {
        public Dictionary<string, string> headers;
        public byte[] payload;
        public Message()
        {
            headers = new Dictionary<string, string>();
            payload = new byte[0];
        }

    }
}