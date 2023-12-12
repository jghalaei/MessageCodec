using System.Text;

namespace MessageCodec
{
    public class SimpleMessageCodec : MessageCodec
    {
        private const byte CURRENT_VERSION = 1;
        private const int MAX_HEADER_LENGTH = 1023;
        private const int MAX_HEADER_COUNT = 63;
        private const int MAX_PAYLOAD_LENGTH = 256 * 1024;
        public byte[] Encode(Message message)
        {
            if (message.headers.Count == 0 && message.payload.Length == 0)
                throw new ArgumentException("Message is empty");
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(CURRENT_VERSION);
                    WriteHeaders(message, writer);
                    WritePayload(message, writer);

                    byte checksum = ComputeChecksum(ms.ToArray());
                    writer.Write(checksum);

                    return ms.ToArray();
                }
            }
        }

        public Message Decode(byte[] data)
        {

            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    ReadAndValidateVersion(reader);
                    ReadAndValidateChecksum(data);
                    Message message = new Message();
                    message.headers = ReadHeaders(reader);
                    message.payload = ReadPayload(reader);
                    return message;
                }
            }

        }
        private void WriteHeaders(Message message, BinaryWriter writer)
        {
            if (message.headers.Count > MAX_HEADER_COUNT)
                throw new ArgumentException($"Header count can not be more than {MAX_HEADER_COUNT}");

            writer.Write((byte)message.headers.Count);
            foreach (var header in message.headers)
            {
                byte[] keyBytes = Encoding.ASCII.GetBytes(header.Key);
                byte[] valueBytes = Encoding.ASCII.GetBytes(header.Value);

                if (keyBytes.Length > MAX_HEADER_LENGTH || valueBytes.Length > MAX_HEADER_LENGTH)
                    throw new ArgumentException($"Header key or value length can not be more than {MAX_HEADER_LENGTH} bytes");

                writer.Write((ushort)keyBytes.Length);
                writer.Write(keyBytes);
                writer.Write((ushort)valueBytes.Length);
                writer.Write(valueBytes);
            }
        }

        private void WritePayload(Message message, BinaryWriter writer)
        {
            if (message.payload.Length > MAX_PAYLOAD_LENGTH)
                throw new ArgumentException($"Payload length can not be more than {MAX_PAYLOAD_LENGTH} bytes");

            writer.Write((int)message.payload.Length);
            writer.Write(message.payload);
        }

        private void ReadAndValidateVersion(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            if (version != CURRENT_VERSION)
                throw new ArgumentException("Unsupported version");
        }
        private void ReadAndValidateChecksum(byte[] data)
        {
            byte checksumFromData = data[data.Length - 1];
            byte computedChecksum = ComputeChecksum(data.Take(data.Length - 1).ToArray());
            if (checksumFromData != computedChecksum)
                throw new ArgumentException("Checksum mismatch");
        }

        private Dictionary<string, string> ReadHeaders(BinaryReader reader)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            var headerCount = reader.ReadByte();

            for (int i = 0; i < headerCount; i++)
            {
                ushort keyLength = reader.ReadUInt16();
                byte[] keyBytes = reader.ReadBytes(keyLength);
                ushort valueLength = reader.ReadUInt16();
                byte[] valueBytes = reader.ReadBytes(valueLength);

                var key = Encoding.ASCII.GetString(keyBytes);
                var value = Encoding.ASCII.GetString(valueBytes);

                headers.Add(key, value);
            }
            return headers;
        }

        private byte[] ReadPayload(BinaryReader reader)
        {
            int payloadLength = reader.ReadInt32();
            byte[] payload = reader.ReadBytes(payloadLength);
            return payload;
        }

        private byte ComputeChecksum(byte[] bytes)
        {
            int sum = bytes.Sum(b => (int)b);
            return unchecked((byte)sum); // Cast the sum to a byte. naturally performs a modulo 256 due to overflow
        }
    }
}