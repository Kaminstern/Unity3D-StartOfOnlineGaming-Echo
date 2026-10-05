using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

namespace EchoClient
{
    public class Echo : MonoBehaviour
    {
        // 定义套接字
        Socket socket;
        // UGUI
        public TextMeshProUGUI inputField;
        public TextMeshProUGUI text;

        // 接收缓冲区
        ByteArray readBuff = new ByteArray();

        string recvStr = "";

        // 判断当前是否需要关闭连接（此时可能还有没法完的数据，不能直接调用socket.Close()
        bool isClosing = false;

        // 定义缓冲区队列
        Queue<ByteArray> writeQueue = new Queue<ByteArray>();

        private void Awake()
        {

        }

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
                socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
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
                readBuff.writeIdx += count;
                // 处理二进制消息
                OnReceiveData();
                // 等待，模拟粘包
                //System.Threading.Thread.Sleep(1000 * 10);

                // 继续接收数据（因为直接操作bytes，而不是用Write，所以需要手动维护扩容）
                // 由于不知道下一次接收的数据量，需要判断缓冲区大小是否有一定的余量
                if(readBuff.remain < 8)
                {
                    readBuff.MoveBytes();
                    readBuff.ReSize(readBuff.length * 2);
                }

                // 等下一个数据过来
                socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
            }
            catch (SocketException ex)
            {
                Debug.Log($"Socket Recive fail: {ex.Message}");
            }
        }

        public void OnReceiveData()
        {
            Debug.Log($"[Revc 1] length = {readBuff.length}");
            Debug.Log($"[Revc 2] readBuff = {readBuff.ToString()}");
            // 消息长度
            if (readBuff.length < 2)       // 不足“消息长度”的长度
            {
                return;
            }
            // 手动处理使用小端方式发过来的“消息长度”
            Int16 bodyLength = readBuff.ReadInt16();
            Debug.Log($"[Recv 3] bodyLength = {bodyLength}");
            // 消息体
            if (readBuff.length < bodyLength)      // 不足“消息长度”的长度加上“消息”的长度（readBuff.ReadInt16();中已经将“消息长度”的部分读完了，剩下的就是纯消息体，不用再加“消息长度”的长度）
            {
                return;
            }
            byte[] stringByte = new byte[bodyLength];
            readBuff.Read(stringByte, 0, bodyLength);
            string s = System.Text.Encoding.UTF8.GetString(stringByte);
            Debug.Log($"[Recv 4] s = {s}");

            Debug.Log($"[Recv 5] readBuff = {readBuff.ToString()}");
            // 消息处理
            recvStr = s + "\n" + recvStr;
            // 继续读消息
            if(readBuff.length > 2)
            {
                OnReceiveData();
            }
        }

        // 点击发送按钮
        public void Send()
        {
            // 已经调用了socket.Close()，不让再给缓存队列添加新内容了
            if (isClosing)
            {
                return;
            }
            // Send
            string sendStr = inputField.text.Replace("\u200B", "");     // 去掉TMP的零宽字符
            Debug.Log($"sendStr: {sendStr}, len: {sendStr.Length}");
            // 组装协议
            byte[] bodyBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            Int16 len = (Int16)bodyBytes.Length;
            byte[] lenBytes = BitConverter.GetBytes(len);
            // 手动判断大小端编码，使用小端存储，如果不是，则需要翻转Reverse
            //（BitConverter.GetBytes中已经做了IsLittleEndian的判断，根据所处机型自动调整，只不过这里需要统一服务端和客户端的“消息长度”存储方式）
            if (!BitConverter.IsLittleEndian)
            {
                Debug.Log("[Send] Reverse lenBytes");
                lenBytes = (byte[])lenBytes.Reverse();
            }

            byte[] sendBytes = lenBytes.Concat(bodyBytes).ToArray();
            ByteArray ba = new ByteArray(sendBytes);

            int count = 0;
            // 避免同一数据被发送多次
            lock (writeQueue)
            {
                writeQueue.Enqueue(ba);
                count = writeQueue.Count;
            }

            if (count == 1)
            {
                socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
            }

            Debug.Log($"[Send] {BitConverter.ToString(ba.bytes)}");
        }

        // Send回调函数
        public void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                // EndSend的处理
                int count = socket.EndSend(ar);             // 只是成功发到了操作系统的发送缓冲区中，由操作系统负责完成数据的发送、确认、重传等步骤（所以可能出现粘包的情况）
                // 判断发送是否完整
                ByteArray ba;
                lock (writeQueue)
                {
                    ba = writeQueue.First();
                }
                ba.readIdx += count;
                if (ba.length == 0)
                {
                    lock (writeQueue)
                    {
                        writeQueue.Dequeue();
                        ba = writeQueue.First();
                    }
                }
                if (ba != null)
                {
                    socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
                }
                else if(isClosing)
                {
                    socket.Close();
                }
            }
            catch (SocketException ex)
            {
                Debug.Log($"Socket Send fail {ex.Message}");
            }
        }

        // 关闭连接
        public void Close()
        {
            // 还有数据在缓冲队列中，没有发完
            if(writeQueue.Count > 0)
            {
                isClosing = true;
            }
            else
            {
                socket.Close();
            }
        }
    }
}
