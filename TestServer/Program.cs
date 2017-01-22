using AsynchronousServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener(new TestHandler());
            listener.StartListening();

        }
    }

    class TestHandler : IMessageHandler
    {
        public string HandleMessage(string input)
        {
            Console.WriteLine("Received Message" + input);
            return input;
        }
    }

}
