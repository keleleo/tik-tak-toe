using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;
namespace Server2
{
    class Server
    {
        public static int dataSize = 4096;

        public static Server instance;

        static TcpListener server = null;
        static int port = 13000;
        IPAddress localAddr = IPAddress.Parse("192.168.0.107");

        public Byte[] receiveDataB;
        public string receiveDataS;
        public List<TcpClient> clients = new List<TcpClient>();

        #region Events
        public delegate void ClientConnected(TcpClient client);
        public delegate void ClientDisConnected(TcpClient client);
        public delegate void MessageReceived(string message, TcpClient client);

        public event ClientConnected clientConnected;
        public event ClientDisConnected clientDisconnected;
        public event MessageReceived messageReceived;

        #endregion
        public Server()
        {
            Server.instance = this;
        }
        public void Start()
        {

            try
            {
                Console.WriteLine("Starting Server . . .");

                server = new TcpListener(localAddr, port);
                server.Start();

                server.BeginAcceptTcpClient(new AsyncCallback(AcceptClients), server);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error:\n{ex}");

            }

            //-----------
            while (true)
            {
                string line = Console.ReadLine();
                if (line == "exit")
                {
                    break;
                }
                else
                {
                    if (line == "clean")
                        Console.Clear();
                    else if (line == "players")
                        ServerManager.instance.SeePlayersConnected();
                    else if (line == "games")
                        ServerManager.instance.SeeGames();
                }
            }
            //-----------

        }
        private void AcceptClients(IAsyncResult _result)
        {
            try
            {
                TcpListener listener = (TcpListener)_result.AsyncState;

                TcpClient client = listener.EndAcceptTcpClient(_result);

                clientConnected?.Invoke(client);
                Console.WriteLine($"New Client: {client.Client.RemoteEndPoint}");

                NetworkStream stream = client.GetStream();


                receiveDataB = new Byte[dataSize];
                server.BeginAcceptTcpClient(new AsyncCallback(AcceptClients), server);
                stream.BeginRead(receiveDataB, 0, dataSize, OnReceive, client);
            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex);
            }
        }
        private void OnReceive(IAsyncResult _result)
        {
            TcpClient client = (TcpClient)_result.AsyncState;
            try
            {
                NetworkStream stream = client.GetStream();
                int byteArray = stream.EndRead(_result);

                byte[] bytes = null;
                Array.Resize(ref bytes, byteArray);
                Buffer.BlockCopy(receiveDataB, 0, bytes, 0, byteArray);

                if (byteArray == 0)
                {
                    Console.WriteLine("error");
                    ClientError(client, "OnReceive: byteArray 0");
                    return;
                }
                try
                {
                    stream.BeginRead(receiveDataB, 0, dataSize, OnReceive, client);
                    receiveDataS = Encoding.ASCII.GetString(bytes);
                    messageReceived?.Invoke(receiveDataS, client);
                }
                catch (Exception _ex)
                {
                    if(bytes[0].ToString() == "0")
                    {
                        ServerManager.instance.Send(client, Communication.Type.errorJson);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientError(client, $"OnReceive: exception -- BIG BUG");
                //Console.WriteLine($"OnReceive error:  {ex}");
            }
        }

        public void Send(TcpClient client, String msg)
        {
            Byte[] data = Encoding.ASCII.GetBytes(msg);
            try
            {
                if (client == null)
                    return;
                Console.WriteLine($"Sending to: {client.Client.RemoteEndPoint}, {msg}-----{data}");

                if (client.Client == null)
                {
                    ClientError(client, "Send: client null");
                    return;
                }
                client.GetStream().Write(data, 0, data.Length);
            }
            catch (System.ObjectDisposedException ex)
            {
                //Console.WriteLine($"Client desconnected: {ex}");
                ClientError(client, "Send: exception");
            }
        }
        private void ClientError(TcpClient client, string from)
        {
            clientDisconnected?.Invoke(client);
        }
    }
}
