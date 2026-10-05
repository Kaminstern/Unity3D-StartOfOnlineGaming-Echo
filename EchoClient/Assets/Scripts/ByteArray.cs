using System;
using System.Collections.Generic;
using System.Text;

namespace EchoClient
{
    public class ByteArray
    {
        // 缓冲区
        public byte[] bytes;
        // 读写位置
        public int readIdx = 0;
        public int writeIdx = 0;
        // 存储数据长度
        public int lenght
        {
            get 
            { 
                return writeIdx - readIdx; 
            }
        }

        public ByteArray(byte[] defaultBytes)
        {
            bytes = defaultBytes;
            readIdx = 0;
            writeIdx = defaultBytes.Length;
        }

        // 打印缓冲区
        public override string ToString()
        {
            return BitConverter.ToString(bytes, readIdx, writeIdx);
        }

        // 打印调试信息
        public string Debug()
        {
            return $"readIdx({readIdx}) writeIdx({writeIdx}) bytes({BitConverter.ToString(bytes, 0, bytes.Length)})";
        }
    }
}
