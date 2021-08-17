using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace server
{
    public class NetworkSystem : MonoBehaviour
    {
        public static NetworkSystem instance;
        public static string ip = "192.168.0.107";
        public static int port = 13000;

        public readonly static int dataBufferSize = 4096;
        private int connectionTry = 0;
        private bool isConnected = false;
        private TcpClient client = null;
        private NetworkStream stream;

        public List<string> seeResult = new List<string>();
        private Byte[] receiveDataB;
        private string receiveDataS;

        #region events
        public delegate void ConnectionErro();
        public delegate void Connected();
        public delegate void DisConnected();
        public delegate void MessageReceived(string message);

        public event ConnectionErro connectionErro;
        public event Connected connected;
        public event DisConnected disConnected;
        public event MessageReceived messageReceived;
        #endregion

        private void Awake()
        {

            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                //Debug.Log("Instance already exists, destroying object!");
                Destroy(this);
            }
        }

        private void Start()
        {
            Connect();
        }

        private void Connect()
        {
            try
            {
                //Connecting . . . 
                // Create new TcpClient
                client = new TcpClient();

                client.BeginConnect(ip, port, ConnectCallback, client);

            }
            catch (Exception ex)
            {
                //***
                //Connection Error
                //***
            }
            finally
            {

            }


        }
        private void ConnectCallback(System.IAsyncResult _reuslt)
        {

            if (!client.Connected)
            {
                //Connection erro
                connectionTry++;
                connectionErro?.Invoke();
                if (!(connectionTry < 11))
                {
                    ClientError();
                    return;
                }
                Connect();
                return;
            }

            client.EndConnect(_reuslt);

            connected?.Invoke();
            isConnected = true;
            // Get stream from client
            stream = client.GetStream();
            receiveDataB = new byte[256];
            stream.BeginRead(receiveDataB, 0, 256, new AsyncCallback(OnReceive), client);
        }

        private void OnReceive(IAsyncResult _result)
        {
            try
            {
                if (!client.Connected)
                    return;
                int byteArray = stream.EndRead(_result);
                byte[] bytes = null;

                Array.Resize(ref bytes, byteArray);
                Buffer.BlockCopy(receiveDataB, 0, bytes, 0, byteArray);
                if (byteArray == 0)
                {
                    ClientError();
                    return;
                }
                string tempReceivedMessage = Encoding.ASCII.GetString(bytes);
                List<string> splitReceivedMessage = tempReceivedMessage.Split('@').ToList();
                splitReceivedMessage.RemoveAt(splitReceivedMessage.Count - 1);

                
                splitReceivedMessage.ForEach(message =>
                {
                    messageReceived?.Invoke(message);
                });
                
                if (splitReceivedMessage.Count > 1)
                {
                    Debug.LogError($"wtf");
                }

                seeResult.Clear();
                seeResult = splitReceivedMessage;
                /*
                receiveDataS = Encoding.ASCII.GetString(bytes);
                Debug.Log(receiveDataS);
                receiveDataS = receiveDataS.Remove(receiveDataS.Length - 1);

                Debug.Log(receiveDataS);
                messageReceived?.Invoke(receiveDataS);
                */
                stream.BeginRead(receiveDataB, 0, 256, new AsyncCallback(OnReceive), client);
            }
            catch (Exception _ex)
            {
                Debug.Log(_ex);
            }
        }

        public void Write(string msg)
        {
            Byte[] data = Encoding.ASCII.GetBytes(msg);
            try
            {
                if (stream != null)
                {
                    stream.Write(data, 0, data.Length);
                    return;
                }
                else
                    stream = client.GetStream();

                if (stream != null)
                {
                    stream.Write(data, 0, data.Length);
                    return;
                }
                else
                    Debug.Log("stream null 2");
            }
            catch (Exception ex)
            {
                if (isConnected)
                    ClientError();
                Debug.Log($"Write error: {ex}");
            }
        }

        public long Ping()
        {
            //not tested
            long pingTime = 0;
            if (client == null || client.Client == null)
            {
                Debug.LogError("Client null");
                return 0;
            }
            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();

            IPAddress server = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

            System.Net.NetworkInformation.PingReply reply = pingSender.Send(server);

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                pingTime = reply.RoundtripTime;
            }
            return pingTime;
        }

        private void ClientError()
        {
            if (client != null)
                client.Close();
            disConnected?.Invoke();
        }

        private void OnApplicationQuit()
        {
            if (stream != null)
                stream.Close();
            client.Close();
        }
    }
}