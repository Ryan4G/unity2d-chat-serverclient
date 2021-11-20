using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    public class AsyncServer
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

            serverSocket.BeginAccept(AcceptCallback, serverSocket);

            Console.ReadLine();
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("[ Server ] Accept");

                Socket server = ar.AsyncState as Socket;
                Socket client = server.EndAccept(ar);

                ClientState clientState = new ClientState();
                clientState.socket = client;
                clients.Add(client, clientState);

                // wait data receive
                client.BeginReceive(clientState.readBuff, 0, 1024, 0, ReceiveCallback, clientState);

                // wait accept receive
                serverSocket.BeginAccept(AcceptCallback, serverSocket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket Accept Failed : {ex}");
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                ClientState clientState = ar.AsyncState as ClientState;
                Socket client = clientState.socket;

                int count = client.EndReceive(ar);

                if (count == 0)
                {
                    client.Close();
                    clients.Remove(client);
                    Console.WriteLine("Socket Close");
                    return;
                }

                string readStr = System.Text.Encoding.UTF8.GetString(clientState.readBuff, 0, count);

                Console.WriteLine($"[ Server ] {readStr}");

                byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(readStr);

                foreach(var clientSend in clients.Keys)
                {
                    clientSend.Send(sendBytes);
                }

                client.BeginReceive(clientState.readBuff, 0, 1024, 0, ReceiveCallback, clientState);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket Receive Failed : {ex}");
            }
        }
    }
}
