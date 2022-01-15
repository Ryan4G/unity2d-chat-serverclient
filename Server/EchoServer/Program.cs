using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncServer.Start("127.0.0.1", 8888);
        }
    }
}
