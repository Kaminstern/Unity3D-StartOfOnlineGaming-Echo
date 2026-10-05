using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Echo : MonoBehaviour
{
    // 定义套接字
    Socket socket;
    // UGUI
    public TextMeshProUGUI inputField;
    public TextMeshProUGUI text;

    // 接收缓冲区
    byte[] readBuff = new byte[1024];
    // 接收缓冲区长度
    int buffCount = 0;
    string recvStr = "";

    List<Socket> checkRead = new List<Socket>();

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
            socket.BeginReceive(readBuff, buffCount, 1024 - buffCount, 0, ReceiveCallback, socket);
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
            // 获取接收数据长度
            int count = socket.EndReceive(ar);
            buffCount += count;
            // 处理二进制消息
            OnReceiveData();
            // 等待，模拟粘包
            System.Threading.Thread.Sleep(1000 * 30);
            string s = System.Text.Encoding.Default.GetString(readBuff, 0, count);

            // 等下一个数据过来
            socket.BeginReceive(readBuff, buffCount, 1024 - buffCount, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Recive fail: {ex.Message}");
        }
    }

    public void OnReceiveData()
    {
        Debug.Log($"[Revc 1] buffCount = {buffCount}");
        Debug.Log($"[Revc 2] readBuff = {BitConverter.ToString(readBuff)}");
        // 消息长度
        if (buffCount < 2)       // 不足“消息长度”的长度
        {
            return;
        }
        Int16 bodyLength = BitConverter.ToInt16(readBuff, 0);
        Debug.Log($"[Recv 3] bodyLength = {bodyLength}");
        // 消息体
        if (buffCount < bodyLength + 2)      // 不足“消息长度”的长度加上“消息”的长度
        {
            return;
        }
        string s = System.Text.Encoding.UTF8.GetString(readBuff, 2, bodyLength);
        Debug.Log($"[Recv 4] s = {s}");
        //更新缓冲区
        int start = 2 + bodyLength;
        buffCount -= start;
        Array.Copy(readBuff, start, readBuff, 0, buffCount);
        Debug.Log($"[Recv 5] = buffCount = {buffCount}");
        recvStr = s + "\n" + recvStr;
        // 继续读消息
        OnReceiveData();
    }

    // 点击发送按钮
    public void Send()
    {
        // Send
        string sendStr = inputField.text.Replace("\u200B", "");     // 去掉TMP的零宽字符
        Debug.Log($"sendStr: {sendStr}, len: {sendStr.Length}");
        // 组装协议
        byte[] bodyBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        Int16 len = (Int16)bodyBytes.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] sendBytes = lenBytes.Concat(bodyBytes).ToArray();
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        Debug.Log($"[Send] {BitConverter.ToString(sendBytes)}");
    }

    // Send回调函数
    public void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);             // 只是成功发到了操作系统的发送缓冲区中，由操作系统负责完成数据的发送、确认、重传等步骤（所以可能出现粘包的情况）
            Debug.Log($"Socket Send succ {count}");
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket Send fail {ex.Message}");
        }
    }
}
