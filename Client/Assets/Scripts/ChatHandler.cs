using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityNetwork;

public class ChatHandler : MyEventHandler
{
    TCPPeer peer = null;
    Socket socket = null;

    public void ConnectToServer()
    {
        peer = new TCPPeer(this);
        socket = peer.Connect("127.0.0.1", 8000);
    }

    public void SendMessage(Packet packet)
    {
        TCPPeer.Send(socket, packet);
    }

}
