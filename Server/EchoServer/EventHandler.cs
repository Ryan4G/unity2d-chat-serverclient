using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    public static class EventHandler
    {
        public static void OnDisconnect(ClientState c)
        {
            Console.WriteLine($"[ Server ] OnDisconnect <- {c.socket.RemoteEndPoint}");

            var sendStr = $"Leave|{c.socket.RemoteEndPoint}";

            foreach (ClientState cs in SelectSyncServer.clients.Values)
            {
                if (cs == c)
                {
                    continue;
                }

                SelectSyncServer.Send(cs, sendStr);
            }

        }
    }
}
