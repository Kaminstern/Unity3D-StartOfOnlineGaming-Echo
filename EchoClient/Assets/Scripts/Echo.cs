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

    // 点击连接按钮
    public void Connection()
    {
        // Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Connect
        socket.Connect("127.0.0.1", 8888);
    }

    // 点击发送按钮
    public void Send()
    {
        // Send
        string sendStr = inputField.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);

        // Recv
        byte[] readBuff = new byte[1024];
        int count = socket.Receive(readBuff);
        string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);

        text.text = recvStr;

        // Close
        socket.Close();
    }
}
