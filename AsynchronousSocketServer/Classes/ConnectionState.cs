using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsynchronousSocketServer.Classes
{
    /// <summary>
    /// Contains the state of the various connections between the server and the clients
    /// </summary>
    public class ConnectionState
    {
        public Socket Connection = null;

        public const int BufferSizw = 1024;

        public byte[] buffer = new byte[BufferSizw];

        public StringBuilder responsebuilder = new StringBuilder();

    }
}
