using System;
using System.Threading.Tasks;

namespace Server2
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            ServerManager manager = new ServerManager();
            
            manager.Start();
            server.Start();
        }
    }

}
