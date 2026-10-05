using EchoClient;
using System;
using UnityEngine;

public class TestByteArray : MonoBehaviour
{
    
    void Start()
    {
        // [1 create]
        ByteArray buff = new ByteArray(8);
        Debug.Log($"[1 debug ] → {buff.Debug()}");
        Debug.Log($"[1 string] → {buff.ToString()}");

        // [2 write]
        byte[] wb = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        buff.Write(wb, 0, 5);
        Debug.Log($"[2 debug ] → {buff.Debug()}");
        Debug.Log($"[2 string] → {buff.ToString()}");

        // [3 read]
        byte[] rb = new byte[4];
        buff.Read(rb, 0, 2);
        Debug.Log($"[3 debug ] → {buff.Debug()}");
        Debug.Log($"[3 string] → {buff.ToString()}");
        Debug.Log($"[3 rb    ] → {BitConverter.ToString(rb)}");

        // [4 write resize]
        wb = new byte[] { 9, 10, 11, 12, 13, 14, 15 };
        buff.Write(wb, 0, wb.Length);
        Debug.Log($"[4 debug ] → {buff.Debug()}");
        Debug.Log($"[4 string] → {buff.ToString()}");
    }

    void Update()
    {
        
    }
}
