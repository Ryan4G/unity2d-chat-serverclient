using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class Echo : MonoBehaviour
{
    Socket socket;

    public InputField inputField;

    public Text text;

    public Text userName;

    private readonly string ip = "127.0.0.1";
    private readonly int port = 8888;

    private bool connected = false;

    private byte[] readBuff = new byte[1024];
    string recvStr = "";

    private void Awake()
    {
        userName.text = $"UID-{new System.Random().Next(10000, 99999)}";
    }

    private void OnDestroy()
    {
        if (socket != null)
        {
            socket.Disconnect(false);
        }
    }
    public void Connection()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Successful");

            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Connect Failed: {ex}");
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;

            var count = socket.EndReceive(ar);

            recvStr = $"{DateTime.Now:G}\n" + System.Text.Encoding.UTF8.GetString(readBuff, 0, count) + $"\n{recvStr}";

            //Debug.Log(recvStr);

            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Receive Failed: {ex}");
        }
    }

    public void Send()
    {
        string sendStr = $"{userName.text}:{inputField.text}";

        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;

            var count = socket.EndSend(ar);

            Debug.Log($"Socket Send {count} bytes");
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Send Failed: {ex}");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = recvStr;
    }
}
