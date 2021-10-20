using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityNetwork;

public class ChatClient : MonoBehaviour
{
    public enum MessageID
    {
        Chat = 100,
    }

    ChatHandler eventHandler;

    public Text txt_receive;
    public InputField input_name;
    public InputField input_message;
    public Button btn_sendmsg;

    // Start is called before the first frame update
    void Start()
    {
        eventHandler = new ChatHandler();

        eventHandler.AddHandler((short)TCPPeer.MessageID.OnConnected, OnConnected);
        eventHandler.AddHandler((short)TCPPeer.MessageID.OnConnectFail, OnConnectFail);
        eventHandler.AddHandler((short)TCPPeer.MessageID.OnDisconnect, OnLost);
        eventHandler.AddHandler((short)MessageID.Chat, OnChat);

        eventHandler.ConnectToServer();

        btn_sendmsg.onClick.AddListener(() => {
            SendChat();
        });
    }

    private void OnChat(Packet packet)
    {
        byte[] buffer = null;
        using (MemoryStream stream = packet.Stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            int len = reader.ReadInt32();
            buffer = reader.ReadBytes(len);
        }

        ChatProto chat = Packet.Deserialize<ChatProto>(buffer);
        printText($"{chat.userName}:{chat.chatMsg}");
    }

    private void OnLost(Packet packet)
    {
        printText("Lost connection..");
    }

    private void OnConnectFail(Packet packet)
    {
        printText("Connect server failed, please exit.");
    }

    private void OnConnected(Packet packet)
    {
        printText("Connect server succeed.");
    }

    private void SendChat()
    {
        ChatProto proto = new ChatProto();

        proto.userName = input_name.text;
        proto.chatMsg = input_message.text;

        byte[] bs = Packet.Serialize(proto);

        Packet p = new Packet((short)MessageID.Chat);

        using(MemoryStream stream = p.Stream) {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(bs.Length);
            writer.Write(bs);
            p.EncodeHeader(stream);
        }

        eventHandler.SendMessage(p);

        input_message.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        eventHandler.ProcessPacket();
    }

    private void printText(string msg)
    {
        if (txt_receive.text.Length > 100)
        {
            txt_receive.text = "";
        }

        txt_receive.text += $"{msg}\n";
    }
}
