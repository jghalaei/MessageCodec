namespace MessageCodec
{
    public interface MessageCodec
    {
        byte[] Encode(Message message);
        Message Decode(byte[] data);
    }
}