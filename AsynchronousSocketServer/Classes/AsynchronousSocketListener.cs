using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsynchronousSocketServer.Classes
{
    /// <summary>
    /// Class which listens for a Asynchronous socket client, then hands the message to a handler
    /// </summary>
    public class AsynchronousSocketListener
    {

        public ManualResetEvent socketaccepted = new ManualResetEvent(false);
        public bool KeepRunning = true;

        public void StartListening()
        {
            byte[] bytes = new Byte[1024];

            //Listens on the local endpoint
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Socket listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

            try
            {


                listener.Bind(localEndPoint);
                //Accept Max 100 listeners
                listener.Listen(100);

                while (KeepRunning)
                {
                    //reset event to nonsignaled state
                    socketaccepted.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallBack),listener);

                    socketaccepted.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Accepts the callback when a new connection is received
        /// Spins off a handler to received the incoming messages
        /// </summary>
        /// <param name="result"></param>
        public void AcceptCallBack(IAsyncResult result)
        {
            socketaccepted.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)result.AsyncState;
            Socket handler = listener.EndAccept(result);

            ConnectionState state = new ConnectionState();
            state.Connection = handler;
            handler.BeginReceive(state.buffer, 0, ConnectionState.BufferSize, 0, new AsyncCallback(ReadCallBack), state);

        }


        public void ReadCallBack(IAsyncResult result)
        {

        }



    }
}
