using System;
using System.Text;
using System.Net.Sockets;

namespace JsonRPCTest.Classes
{
    internal class SocketStateObject
    {
        public const int BufferSize = 1024;

        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder Builder = new StringBuilder();
    }
}
