using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace Empyrion_Mod_Server.client
{
    class Client
    {
        List<ModProtocol.Package> receivedPackages = new List<ModProtocol.Package>();
        List<ModProtocol.Package> receivedPackagesTemp = new List<ModProtocol.Package>();
        volatile ModProtocol client;
        const string cIP = "127.0.0.1";
        const int cPort = 12345;

        private void ThreadConnectToServer(ModThreadHelper.Info ti)
        {
            Console.Write(string.Format("ModInterface: Started connection thread. Connecting to {0}:{1}", cIP, cPort));
            while (!ti.eventRunning.WaitOne(0))
            {
                if (client == null)
                {
                    try
                    {
                        TcpClient tcpClient = new TcpClient(cIP, cPort);
                        client = new ModProtocol(tcpClient, PackageReceivedDelegate, DisconnectedDelegate);
                       Console.WriteLine("ModInterface: Connected with " + client);
                    }
                    catch (SocketException)
                    {
                        // Ignore
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.GetType() + ": " + e.Message);
                        client = null;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        // Called in a thread!
        private void PackageReceivedDelegate(ModProtocol con, ModProtocol.Package p)
        {
            //Console.WriteLine(string.Format("Package id rec: {0}", p.cmd));
            lock (receivedPackages)
            {
                receivedPackages.Add(p);
            }
        }

        // Called in a thread!
        private void DisconnectedDelegate(ModProtocol prot)
        {
            Console.WriteLine("DisconnectedDelegate called");
            client.Close();
            client = null;
        }
    }
}
