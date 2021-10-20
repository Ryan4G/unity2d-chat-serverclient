using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityNetwork;

namespace ChatServer
{
    public class ChatServer : MyEventHandler
    {
        public enum MessageID
        {
            Chat = 100
        }

        private TCPPeer peer;
        private List<Socket> peerList;
        private Thread thread;
        private bool isRunning = false;
        protected EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        public ChatServer()
        {
            peerList = new List<Socket>();
        }

        public void RunServer(int port)
        {
            AddHandler((short)TCPPeer.MessageID.OnNewConnection, OnAccepted);
            AddHandler((short)TCPPeer.MessageID.OnDisconnect, OnLost);
            AddHandler((short)MessageID.Chat, OnChat);

            peer = new TCPPeer(this);
            peer.Listen(port);

            isRunning = true;

            thread = new Thread(UpdateHandler);
            thread.Start();

            Console.WriteLine("Start Chat Server...");
        }

        private void OnChat(Packet packet)
        {
            string message = string.Empty;
            byte[] bs = null;

            using (MemoryStream stream = packet.Stream)
            {
                try
                {
                    BinaryReader reader = new BinaryReader(stream);
                    int byteLen = reader.ReadInt32();
                    bs = reader.ReadBytes(byteLen);

                    ChatProto chat = Packet.Deserialize<ChatProto>(bs);
                    Console.WriteLine($"{chat.userName}:{chat.chatMsg}");
                }
                catch
                {
                    return;
                }
            }

            Packet response = new Packet((short)MessageID.Chat);
            using (MemoryStream stream = response.Stream)
            {
                try
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(bs.Length);
                    writer.Write(bs);
                    response.EncodeHeader(stream);
                }
                catch
                {
                    return;
                }
            }

            // Boardcast all clients
            foreach (Socket sk in peerList)
            {
                TCPPeer.Send(sk, response);
            }
        }

        private void OnLost(Packet packet)
        {
            Console.WriteLine($"Lost connection...{packet.sk.RemoteEndPoint}");
            peerList.Remove(packet.sk);
        }

        private void OnAccepted(Packet packet)
        {
            Console.WriteLine($"Accept new connection...{packet.sk.RemoteEndPoint}");
            peerList.Add(packet.sk);
        }

        private void UpdateHandler()
        {
            while (isRunning)
            {
                waitHandle.WaitOne(-1);

                ProcessPacket();
            }
            thread.Join();
            Console.WriteLine("Close Event Thread...");
        }

        public override void AddPacket(Packet packet)
        {
            lock (Packets)
            {
                Packets.Enqueue(packet);

                waitHandle.Set();
            }
        }
    }
}
