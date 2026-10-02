using System;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Echo : MonoBehaviour
{
    // 定义套接字
    Socket socket;
    // UGUI
    public TextMeshProUGUI inputField;
    public TextMeshProUGUI text;

    // 接收缓冲区
    byte[] readBuff = new byte[1024];
    string recvStr = "";

    public void Update()
    {
        // Unity中，只有主线程可以操作UI组件，所以ReceiveCallback只给recvStr赋值，主线程执行Update的时候再给Text赋值
        text.text = recvStr;
    }

    // 点击连接按钮
    public void Connection()
    {
        // Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Connect
        socket.BeginConnect("127.0.0.1", 8888, ConnectCallback, socket);
    }

    // Connect回调函数
    public void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Connect fail: {ex.Message}");
        }
    }

    // Receive回调函数
    public void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            string s = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            recvStr = s + "\n" + recvStr;
            // 等下一个数据过来
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Recive fail: {ex.Message}");
        }
    }

    // 点击发送按钮
    public void Send()
    {
        // Send
        string sendStr = inputField.text.Replace("\u200B", "");
        Debug.Log($"sendStr: {sendStr}, len: {sendStr.Length}");
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    // Send回调函数
    public void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);             // 只是成功发到了操作系统的发送缓冲区中，由操作系统负责完成数据的发送、确认、重传等步骤
            Debug.Log($"Socket Send succ {count}");
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Send fail {ex.Message}");
        }
    }
}
