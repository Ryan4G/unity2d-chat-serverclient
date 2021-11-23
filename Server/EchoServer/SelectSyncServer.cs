using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace EchoServer
{
    public class SelectSyncServer
    {
        static Socket serverSocket;

        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        public static void Start(string ip, int port)
        {
            Console.WriteLine("[ Server ] Started! ");

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

            serverSocket.Listen(0);

            Console.WriteLine("[ Server ] Launch Successful");

            List<Socket> checkRead = new List<Socket>();

            while (true)
            {
                checkRead.Clear();
                checkRead.Add(serverSocket);

                foreach(Socket client in clients.Keys)
                {
                    checkRead.Add(client);
                }

                // Select Enable Read Socket
                Socket.Select(checkRead, null, null, 1000);

                foreach(Socket s in checkRead)
                {
                    if (s == serverSocket)
                    {
                        ReadListenServer(s);
                    }
                    else
                    {
                        ReadListenClient(s);
                    }
                }

                System.Threading.Thread.Sleep(1);
            }
        }

        private static bool ReadListenClient(Socket s)
        {
            ClientState clientState = clients[s];
            int count = 0;

            try
            {
                count = s.Receive(clientState.readBuff);
            }
            catch (SocketException ex)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] obs = { clientState };
                mei.Invoke(null, obs);

                s.Close();
                clients.Remove(s);
                Console.WriteLine($"[ Server ] Receive SocketException :{ex}");
                return false;
            }

            if (count == 0)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] obs = { clientState };
                mei.Invoke(null, obs);

                Console.WriteLine($"[ Server ] Socket Close <- {s.RemoteEndPoint}");

                s.Close();
                clients.Remove(s);
                return false;
            }

            var recvStr = System.Text.Encoding.UTF8.GetString(clientState.readBuff, 0, count);

            Console.WriteLine($"[ Server ] Receive <- {s.RemoteEndPoint}: {recvStr}");

            var splits = recvStr.Split("|");

            var msgName = splits[0];
            var msgArgs = splits[1];
            string funcName = $"Msg{msgName}";

            MethodInfo mi = typeof(MsgHandler).GetMethod(funcName);
            object[] o = { clientState, msgArgs };
            mi.Invoke(null, o);

            return true;
        }

        private static void ReadListenServer(Socket serverSocket)
        {
            Socket socket = serverSocket.Accept();
            ClientState clientState = new ClientState();
            clientState.socket = socket;
            clients.Add(socket, clientState);

            Console.WriteLine($"[ Server ] Accept <- {socket.RemoteEndPoint}");
        }

        public static void Send(ClientState c, string msg)
        {
            if (c.socket != null && c.socket.Connected)
            {
                try
                {
                    byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(msg);
                    c.socket.Send(sendBytes);

                    Console.WriteLine($"[ Server ] Send -> {c.socket.RemoteEndPoint}: {msg}");
                }
                catch(SocketException ex)
                {
                    Console.WriteLine($"[ Server ] Socket Error: {ex}");
                }
            }
        }
    }
}
