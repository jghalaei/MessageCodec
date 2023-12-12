using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageEncoder
{
    public interface MessageCodec
    {
        byte[] Encode(Message message);
        Message Decode(byte[] data);
    }
}