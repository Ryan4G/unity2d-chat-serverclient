using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
    public static class MsgHandler
    {
        public static void MsgEnter(ClientState c, string msgArgs)
        {
            var split = msgArgs.Split(',');

            var desc = split[0];

            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);
            float eulY = float.Parse(split[4]);

            c.hp = 5;
            c.x = x;
            c.y = y;
            c.z = z;
            c.eulY = eulY;

            var sendStr = $"Enter|{msgArgs}";

            foreach (ClientState cs in SelectSyncServer.clients.Values)
            {
                SelectSyncServer.Send(cs, sendStr);
            }

        }

        public static void MsgList(ClientState c, string msgArgs)
        {
            var sendStr = $"List|";

            List<string> strList = new List<string>();

            foreach (ClientState cs in SelectSyncServer.clients.Values)
            {
                strList.Add(cs.ToString());
            }

            sendStr = $"{sendStr}{string.Join('/', strList)}";

            SelectSyncServer.Send(c, sendStr);
        }

        public static void MsgMove(ClientState c, string msgArgs)
        {
            var split = msgArgs.Split(',');

            var desc = split[0];

            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);

            c.x = x;
            c.y = y;
            c.z = z;

            var sendStr = $"Move|{msgArgs}";

            foreach (ClientState cs in SelectSyncServer.clients.Values)
            {
                SelectSyncServer.Send(cs, sendStr);
            }
        }

        public static void MsgAttack(ClientState c, string msgArgs)
        {
            var sendStr = $"Attack|{msgArgs}";

            foreach (ClientState cs in SelectSyncServer.clients.Values)
            {
                SelectSyncServer.Send(cs, sendStr);
            }
        }

        public static void MsgHurt(ClientState c, string msgArgs)
        {
            var split = msgArgs.Split(',');

            var attackDesc = split[0];

            var hurtDesc = split[1];

            var damage = uint.Parse(split[2]);

            var sendStr = $"Hurt|{msgArgs}";

            ClientState hurtClient = null;

            // find hurt client
            foreach(ClientState cs in SelectSyncServer.clients.Values)
            {
                if (cs.socket.Connected && cs.socket.RemoteEndPoint.ToString() == hurtDesc)
                {
                    hurtClient = cs;
                }
            }

            if (hurtClient == null)
            {
                return;
            }

            hurtClient.hp -= (int)damage;

            if (hurtClient.hp <= 0)
            {
                sendStr = $"Die|{hurtClient.socket.RemoteEndPoint}";
            }

            foreach (ClientState cs in SelectSyncServer.clients.Values)
            {
                SelectSyncServer.Send(cs, sendStr);
            }
        }

        public static void MsgDie(ClientState c, string msgArgs)
        {
            var sendStr = $"Die|{msgArgs}";

            foreach (ClientState cs in SelectSyncServer.clients.Values)
            {
                SelectSyncServer.Send(cs, sendStr);
            }
        }
    }
}
