using System.Net;
using System.Net.Sockets;

namespace EchoServer
{
    /// <summary>
    /// 服务器经历Socket、Bind、Listen三个步骤初始化监听Socket，然后调用BeginAccept开始异步处理客户端连接
    /// 商业上为了做到性能极致，大多使用异步，或使用多线程模拟异步程序。后续书中提到的服务端使用select，尝试改为异步实现
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

            // checkRead
            List<Socket> checkRead = new List<Socket>();

            while (true)
            {
                // 填充checkRead列表
                checkRead.Clear();
                checkRead.Add(listenfd);
                foreach (ClientState s in clients.Values)
                {
                    checkRead.Add(s.socket);
                }

                // select
                Socket.Select(checkRead, null, null, 1000);
                // 检查可读对象
                foreach(Socket s in checkRead)
                {
                    if(s == listenfd)
                    {
                        ReadListenfd(s);
                    }
                    else
                    {
                        ReadClientfds(s);
                    }
                }
            }
        }

        // 应答客户端
        public static void ReadListenfd(Socket listenfd)
        {
            Console.WriteLine("[服务器] Accept");
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }

        // 接收客户端消息，并广播给所有客户端
        public static bool ReadClientfds(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            // 接收
            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuff);
            }
            catch(SocketException ex)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine($"Receive SocketException {ex.Message}");
                return false;
            }
            // 客户端关闭
            if(count == 0)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Socket close");
                return false;
            }

            // 广播
            string revcStr = System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
            Console.WriteLine($"Receive {revcStr}");
            string sendStr = clientfd.RemoteEndPoint.ToString() + ":" + revcStr;
            byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            foreach(ClientState s in clients.Values)
            {
                s.socket.Send(sendBytes);
            }
            return true;
        }
    }
}