using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnityNetwork
{
    public class Packet
    {
        // 0-1 byte save data length
        // 2-3 byte save message id
        public const int headerLength = 4;

        public short msgid = 0;

        public Socket sk = null;

        public byte[] buffer = new byte[1024];

        public int readLength = 0;

        public short bodyLength = 0;
        // mark if buffer header filled up
        public bool encoded = false;

        public Packet(short id, Socket s = null)
        {
            msgid = id;
            sk = s;
            byte[] bs = BitConverter.GetBytes(id);
            bs.CopyTo(buffer, 2);
        }
        public Packet(Packet p)
        {
            msgid = p.msgid;
            sk = p.sk;
            p.buffer.CopyTo(buffer, 0);
            bodyLength = p.bodyLength;
            readLength = p.readLength;
            encoded = p.encoded;
        }

        public void ResetParams()
        {
            msgid = 0;
            readLength = 0;
            bodyLength = 0;
            encoded = false;
        }

        public void EncodeHeader(MemoryStream stream)
        {
            if (stream != null)
            {
                bodyLength = (short)stream.Position;
            }

            byte[] bs = BitConverter.GetBytes(bodyLength);
            bs.CopyTo(buffer, 0);

            encoded = true;
        }

        public void DecodeHeader()
        {
            bodyLength = BitConverter.ToInt16(buffer, 0);
            msgid = BitConverter.ToInt16(buffer, 2);
        }

        public MemoryStream Stream
        {
            get
            {
                return new MemoryStream(buffer, headerLength, buffer.Length - headerLength);
            }
        }

        public static byte[] Serialize<T>(T t)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();

                    bf.Serialize(stream, t);
                    stream.Seek(0, SeekOrigin.Begin);

                    return stream.ToArray();
                }
                catch(Exception e)
                {
                    return null;
                }
            }
        }

        public static T Deserialize<T>(byte[] bs)
        {
            using (MemoryStream stream = new MemoryStream(bs))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    T t = (T)bf.Deserialize(stream);
                    return t;
                }
                catch(Exception e)
                {
                    return default(T);
                }
            }
        }
    }
}
