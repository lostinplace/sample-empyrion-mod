using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using Eleon.Modding;


namespace ENRC.client
{
    public class Client
    {
        ModThreadHelper.Info connectToServerThread;

        public event Action<String> ClientMessages;
        public event Action<ModProtocol.Package> GameEventReceived;

        volatile ModProtocol client;
        const string cIP = "127.0.0.1";
        const int cPort = 12345;

        public Client()
        {
            connectToServerThread = ModThreadHelper.StartThread(ThreadConnectToServer, System.Threading.ThreadPriority.Lowest);
        }

        private void ThreadConnectToServer(ModThreadHelper.Info ti)
        {
            ClientMessages(string.Format("ModInterface: Started connection thread. Connecting to {0}:{1}", cIP, cPort));
            while (!ti.eventRunning.WaitOne(0))
            {
                if (client == null)
                {
                    try
                    {
                       TcpClient tcpClient = new TcpClient(cIP, cPort);
                       client = new ModProtocol(tcpClient, PackageReceivedDelegate, DisconnectedDelegate);
                       ClientMessages("ModInterface: Connected with " + client);
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
                ModProtocol.Package p = new ModProtocol.Package(cmdId, 0, seqNr, data);
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
