using System;
using System.Net.Sockets;
using System.Threading;
using Eleon.Modding;


namespace EPMConnector
{
    public class Client
    {
        ModThreadHelper.Info connectToServerThread;

        public event Action OnConnected;
        public event Action<String> ClientMessages;
        public event Action<ModProtocol.Package> GameEventReceived;

        volatile ModProtocol client;

        int clientId;

        string gameServerIp;
        int gameServerPort;

        public Client(int clientId)
        {
            this.clientId = clientId;
        }

        public void Connect(string ipAddress, int port)
        {
            this.gameServerIp = ipAddress;
            this.gameServerPort = port;
            connectToServerThread = ModThreadHelper.StartThread(ThreadConnectToServer, System.Threading.ThreadPriority.Lowest);
        }

        private void ThreadConnectToServer(ModThreadHelper.Info ti)
        {
            ClientMessages(string.Format("ModInterface: Started connection thread. Connecting to {0}:{1}", this.gameServerIp, this.gameServerPort));
            while (!ti.eventRunning.WaitOne(0))
            {
                if (client == null)
                {
                    try
                    {
                        TcpClient tcpClient = new TcpClient(this.gameServerIp, this.gameServerPort);
                        tcpClient.ReceiveBufferSize = 10 * 1024 * 1024;
                        tcpClient.SendBufferSize = 10 * 1024 * 1024;

                        client = new ModProtocol(tcpClient, PackageReceivedDelegate, DisconnectedDelegate);
                        ClientMessages("ModInterface: Connected with " + client + " over port " + this.gameServerPort);

                        OnConnected?.Invoke();
                    }
                    catch (SocketException)
                    {
                        // Ignore
                    }
                    catch (Exception e)
                    {
                        ClientMessages(e.GetType() + ": " + e.Message);
                        client = null;
                    }
                }
                Thread.Sleep(1000);
            }
        }
        
        // Called in a thread!
        private void PackageReceivedDelegate(ModProtocol con, ModProtocol.Package p)
        {         
            GameEventReceived(p);
        }

        // Called in a thread!
        private void DisconnectedDelegate(ModProtocol prot)
        {
            ClientMessages("DisconnectedDelegate called");
            Disconnect();
        }

        public void Send(CmdId cmdId, ushort seqNr, object data)
        {
            //ClientMessages("Sending request event: c=" + cmdId + " sNr=" + seqNr + " d=" + data + " client="+client);
            // Send events of the network
            ModProtocol c = client;
            if (c != null)
            {
                ModProtocol.Package p = new ModProtocol.Package(cmdId, clientId, seqNr, data);
                c.AddToSendQueue(p);
            }
        }

        public void Disconnect()
        {
            if(client != null)
            {
                client.Close();
                client = null;
            }
            
        }
    }
}
