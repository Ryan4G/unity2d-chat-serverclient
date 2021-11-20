using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    public class SelectSyncServer
    {
        static Socket serverSocket;

        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

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
                s.Close();
                clients.Remove(s);
                Console.WriteLine($"[ Server ] Receive SocketException :{ex}");
                return false;
            }

            if (count == 0)
            {
                s.Close();
                clients.Remove(s);
                Console.WriteLine($"[ Server ] Socket Close");
                return false;
            }

            var recvStr = System.Text.Encoding.UTF8.GetString(clientState.readBuff, 0, count);

            Console.WriteLine($"[ Server ] Receive : {recvStr}");

            var sendStr = $"{s.RemoteEndPoint}:{recvStr}";
            byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);

            foreach(Socket sk in clients.Keys)
            {
                sk.Send(sendBytes);
            }

            return true;
        }

        private static void ReadListenServer(Socket serverSocket)
        {
            Console.WriteLine("[ Server ] Accept");

            Socket socket = serverSocket.Accept();
            ClientState clientState = new ClientState();
            clientState.socket = socket;
            clients.Add(socket, clientState);
        }

    }
}
