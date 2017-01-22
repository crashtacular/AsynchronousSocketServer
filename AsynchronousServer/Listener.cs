using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsynchronousServer
{
    /// <summary>
    /// Class which listens for a Asynchronous socket client, then hands the message to a handler
    /// </summary>
    public class Listener
    {
        //End of Message string, default value
        private string EOF = ";";
        private IMessageHandler _messagehandler;
        private ManualResetEvent _socketaccepted = new ManualResetEvent(false);
        private bool _keepRunning = true;
        private int _port = 11000;

        private Thread _listeningthread;

        #region public methods
        public Listener(IMessageHandler handler)
        {
            _messagehandler = handler;
        }

        /// <summary>
        /// Start the Listener Listening for new connections
        /// </summary>
        public void StartListening()
        {
            _listeningthread = new Thread(Listen);
            _listeningthread.Start();
        }

        /// <summary>
        /// Stop Listening for new connections, but will handle the ones already connected
        /// </summary>
        public void StopListening()
        {
            _keepRunning = false;
        }

        public void SetPort(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Sets the string which will be checked for end of message
        /// </summary>
        /// <param name="eof"></param>
        public void SetEndingString(string eof)
        {
            EOF = eof;
        }
        #endregion

        private void Listen()
        {
            byte[] bytes = new Byte[1024];

            //Listens on the local endpoint
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());



            IPAddress ipAddress = (from ip in ipHostInfo.AddressList
                                   where ip.AddressFamily == AddressFamily.InterNetwork
                                   select ip).FirstOrDefault();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {

                listener.Bind(localEndPoint);
                //Accept Max 100 backlog
                listener.Listen(100);

                while (_keepRunning)
                {
                    //reset event to nonsignaled state
                    _socketaccepted.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);

                    _socketaccepted.WaitOne();
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
        private void AcceptCallBack(IAsyncResult result)
        {
            _socketaccepted.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)result.AsyncState;
            Socket handler = listener.EndAccept(result);

            ConnectionState state = new ConnectionState();
            state.Connection = handler;
            handler.BeginReceive(state.buffer, 0, ConnectionState.BufferSize, 0, new AsyncCallback(ReadCallback), state);

        }

        /// <summary>
        /// Handles the Callback from the connected client
        /// Recursively Called until the entire message is handled
        /// </summary>
        /// <param name="result"></param>
        private void ReadCallback(IAsyncResult result)
        {
            ConnectionState state = (ConnectionState)result.AsyncState;
            Socket handler = state.Connection;
            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(result);

            string content = String.Empty;

            if (bytesRead > 0)
            {
                state.stringbuilder.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.stringbuilder.ToString();
                if (content.IndexOf(EOF) > -1)
                {
                    Send(handler,_messagehandler.HandleMessage(content));
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, ConnectionState.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void Send(Socket handler, String message)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(message);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



    }
}

