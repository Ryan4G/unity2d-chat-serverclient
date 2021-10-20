using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UnityNetwork
{
    public class TCPPeer
    {
        public enum MessageID
        {
            OnNewConnection = 1,
            OnConnected,
            OnConnectFail,
            OnDisconnect
        }

        protected MyEventHandler handler;

        public TCPPeer(MyEventHandler h)
        {
            handler = h;
        }

        // Server
        public void Listen(int port, int backlog = 128)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, port);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(ipe);
                socket.Listen(backlog);

                socket.BeginAccept(new AsyncCallback(ListenCallback), socket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ListenCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;

            try
            {
                Socket client = listener.EndAccept(ar);

                handler.AddPacket(new Packet((short)MessageID.OnNewConnection, client));

                Packet packet = new Packet(0, client);

                // server begin to accept client message
                client.BeginReceive(packet.buffer, 0, Packet.headerLength,
                    SocketFlags.None, new AsyncCallback(ReceiveHeader), packet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // server listen to other client
            listener.BeginAccept(new AsyncCallback(ListenCallback), listener);
        }

        private void ReceiveHeader(IAsyncResult ar)
        {
            Packet packet = (Packet)ar.AsyncState;

            try
            {
                int read = packet.sk.EndReceive(ar);

                if (read < 1)
                {
                    handler.AddPacket(new Packet((short)MessageID.OnDisconnect, packet.sk));
                    return;
                }

                packet.readLength += read;

                if (packet.readLength < Packet.headerLength)
                {

                    packet.sk.BeginReceive(packet.buffer, packet.readLength, Packet.headerLength - packet.readLength,
                        SocketFlags.None, new AsyncCallback(ReceiveHeader), packet);
                }
                else
                {
                    packet.DecodeHeader();
                    packet.readLength = 0;

                    // begin to read body content
                    packet.sk.BeginReceive(packet.buffer, Packet.headerLength, packet.bodyLength,
                        SocketFlags.None, new AsyncCallback(ReceiveBody), packet);
                }
            }
            catch(Exception e)
            {
                handler.AddPacket(new Packet((short)MessageID.OnDisconnect, packet.sk));
            }
        }

        private void ReceiveBody(IAsyncResult ar)
        {
            Packet packet = (Packet)ar.AsyncState;

            try
            {
                int read = packet.sk.EndReceive(ar);

                if (read < 1)
                {
                    handler.AddPacket(new Packet((short)MessageID.OnDisconnect, packet.sk));
                    return;
                }

                packet.readLength += read;

                if (packet.readLength < packet.bodyLength)
                {

                    packet.sk.BeginReceive(packet.buffer, Packet.headerLength + packet.readLength, packet.bodyLength - packet.readLength,
                        SocketFlags.None, new AsyncCallback(ReceiveBody), packet);
                }
                else
                {
                    Packet newPacket = new Packet(packet);

                    handler.AddPacket(newPacket);

                    // reset packet
                    packet.ResetParams();
                    // begin to read next package
                    packet.sk.BeginReceive(packet.buffer, 0, Packet.headerLength,
                        SocketFlags.None, new AsyncCallback(ReceiveHeader), packet);
                }
            }
            catch (Exception e)
            {
                handler.AddPacket(new Packet((short)MessageID.OnDisconnect, packet.sk));
            }
        }

        // Client
        public Socket Connect(string ip, int port)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);

            try
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(ipe, new AsyncCallback(ConnectionCallback), client);

                return client;
            }
            catch (Exception e)
            {
                handler.AddPacket(new Packet((short)MessageID.OnConnectFail));
                return null;
            }
        }

        private void ConnectionCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;

            try
            {
                client.EndConnect(ar);

                handler.AddPacket(new Packet((short)MessageID.OnConnected, client));

                Packet packet = new Packet(0, client);

                // client begin to accept server message
                client.BeginReceive(packet.buffer, 0, Packet.headerLength,
                    SocketFlags.None, new AsyncCallback(ReceiveHeader), packet);
            }
            catch (Exception e)
            {
                handler.AddPacket(new Packet((short)MessageID.OnConnectFail));
            }
        }

        public static void Send(Socket sk, Packet packet)
        {
            if (!packet.encoded)
            {
                throw new Exception("Invalid Data Package!");
            }

            NetworkStream ns;

            lock (sk)
            {
                ns = new NetworkStream(sk);

                if (ns.CanWrite)
                {
                    try
                    {
                        ns.BeginWrite(packet.buffer, 0, Packet.headerLength + packet.bodyLength,
                            new AsyncCallback(SendCallback), ns);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;

            try
            {
                ns.EndWrite(ar);
                ns.Flush();
                ns.Close();
            }
            catch(Exception e) { }
        }
    }
}
