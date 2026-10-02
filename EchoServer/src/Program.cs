using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            // Socket
            Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);

            // Listen
            listenfd.Listen(0);     // 参数backlog表示队列中最多可容纳等待接受的连接数，0表示不限制
            Console.WriteLine("[服务器] 启动成功");
            while (true)
            {
                // Accept
                Socket connfd = listenfd.Accept();          // 阻塞式，客户端如果没有发消息过来，就一直卡在这
                Console.WriteLine("[服务器] Accept");
                // Receive
                byte[] readBuff = new byte[1024];
                int count = connfd.Receive(readBuff);
                string readStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
                Console.WriteLine($"[服务器接收] {readStr}");
                // Send
                string str = $"{System.DateTime.Now.ToString()} 服务器收到了发来的信息：{readStr}";
                Console.WriteLine(str + "  不知道");
                byte[] sendStr = System.Text.Encoding.Default.GetBytes(str);
                connfd.Send(sendStr);
            }
        }
    }
}