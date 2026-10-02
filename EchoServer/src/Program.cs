using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    /// <summary>
    /// 服务器经历Socket、Bind、Listen三个步骤初始化监听Socket，然后调用BeginAccept开始异步处理客户端连接
    /// </summary>
    class MainClass
    {
        // 监听Socket
        static Socket listenfd;
        // 获取所有连接的客户端Socket和状态信息
        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            // Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);

            // Listen
            listenfd.Listen(0);     // 参数backlog表示队列中最多可容纳等待接受的连接数，0表示不限制
            Console.WriteLine("[服务器] 启动成功");

            // Accept
            listenfd.BeginAccept(AcceptCallback, listenfd);          // 阻塞式，客户端如果没有发消息过来，就一直卡在这

            // 等待
            Console.ReadLine();

        }

        // Accept回调函数
        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("[服务器] Accept");
                Socket listenfd = (Socket)ar.AsyncState!;
                Socket clientfd = listenfd.EndAccept(ar);

                // clients列表
                ClientState state = new ClientState();
                state.socket = clientfd;
                clients.Add(clientfd, state);
                // 接收数据BeginReceive
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);

                // 继续Accept
                listenfd.BeginAccept(AcceptCallback, listenfd);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket Accept fail {ex.Message}");
            }
        }

        // Receive回调函数
        public static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                ClientState state = (ClientState)ar.AsyncState!;
                Socket clientfd = state.socket;
                int count = clientfd.EndReceive(ar);

                // 客户端关闭
                if (count == 0)
                {
                    clientfd.Close();
                    clients.Remove(clientfd);
                    Console.WriteLine("Socket close");
                    return;
                }

                string recvStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
                byte[] sendBytes = System.Text.Encoding.Default.GetBytes($"{System.DateTime.Now.ToString()} 服务器收到了发来的信息：{recvStr}");

                clientfd.Send(sendBytes);       // 减少代码量，不用异步
                clientfd.BeginReceive(state.readBuff, 0, 1024, 0, ReceiveCallback, state);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket Receive fail {ex.Message}");
            }
        }
    }
}