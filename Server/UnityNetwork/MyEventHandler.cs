using System;
using System.Collections.Generic;
using System.Text;

namespace UnityNetwork
{
    public class MyEventHandler
    {
        public delegate void OnReceive(Packet packet);

        protected Dictionary<int, OnReceive> handlers;

        protected Queue<Packet> Packets = new Queue<Packet>();

        public MyEventHandler()
        {
            handlers = new Dictionary<int, OnReceive>();
        }

        public virtual void AddHandler(int msgid, OnReceive handler)
        {
            handlers.Add(msgid, handler);
        }

        public virtual void AddPacket(Packet packet)
        {
            lock (Packets) {
                Packets.Enqueue(packet);
            }
        }

        public Packet GetPacket()
        {
            lock (Packets)
            {
                if (Packets.Count == 0)
                {
                    return null;
                }

                return Packets.Dequeue();
            }
        }

        public void ProcessPacket()
        {
            Packet packet = GetPacket();

            while(packet != null)
            {
                OnReceive handler = null;

                if (handlers.TryGetValue(packet.msgid, out handler))
                {
                    if (handler != null)
                    {
                        handler(packet);
                    }
                }

                packet = GetPacket();
            }
        }
    }
}
