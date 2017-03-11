using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using Eleon.Modding;

public class EPM : ModInterface {

    const string cIP = "127.0.0.1";
    const int cPort = 12345;

    ModGameAPI GameAPI;
    //float lastTime;
    //HashSet<int> players = new HashSet<int>();
    ModThreadHelper.Info connectToServerThread;
    volatile ModProtocol client;
    List<ModProtocol.Package> receivedPackages = new List<ModProtocol.Package>();
    List<ModProtocol.Package> receivedPackagesTemp = new List<ModProtocol.Package>();

    public void Game_Start(ModGameAPI gameAPI) {
        GameAPI = gameAPI;
        GameAPI.Console_Write("Game start");
        connectToServerThread = ModThreadHelper.StartThread(ThreadConnectToServer, System.Threading.ThreadPriority.Lowest);
    }

    // Thread
    private void ThreadConnectToServer(ModThreadHelper.Info ti) {
        GameAPI.Console_Write(string.Format("ModInterface: Started connection thread. Connecting to {0}:{1}", cIP, cPort));
        while (!ti.eventRunning.WaitOne(0)) {
            if (client == null) {
                try {
                    TcpClient tcpClient = new TcpClient(cIP, cPort);
                    client = new ModProtocol(tcpClient, PackageReceivedDelegate, DisconnectedDelegate);
                    GameAPI.Console_Write("ModInterface: Connected with " + client);
                } catch (SocketException) {
                    // Ignore
                } catch (Exception e) {
                    GameAPI.Console_Write(e.GetType() + ": " + e.Message);
                    client = null;
                }
            }
            Thread.Sleep(1000);
        }
    }

    // Called in a thread!
    private void PackageReceivedDelegate(ModProtocol con, ModProtocol.Package p) {
        //Console.WriteLine(string.Format("Package id rec: {0}", p.cmd));
        lock (receivedPackages) {
            receivedPackages.Add(p);
        }
    }

    // Called in a thread!
    private void DisconnectedDelegate(ModProtocol prot) {
        GameAPI.Console_Write("DisconnectedDelegate called");
        client.Close();
        client = null;
    }

    public void Game_Update() {

        // Exchange lists
        List<ModProtocol.Package> temp;
        lock (receivedPackages) {
            temp = receivedPackages;
            receivedPackages = receivedPackagesTemp;
            receivedPackagesTemp = temp;
            receivedPackages.Clear();
        }

        // Send request to the game
        for (int i=0; i<temp.Count; i++) {
            GameAPI.Game_Request(temp[i].cmd, temp[i].seqNr, temp[i].data);
        }
        temp.Clear();
    }

    public void Game_Exit() {
        GameAPI.Console_Write("Game exit");
        if (client != null) {
            client.Close();
        }
        connectToServerThread.WaitForEnd();
    }

    public void Game_Event(CmdId cmdId, ushort seqNr, object data) {
        //GameAPI.Console_Write("Game event: c=" + cmdId + " sNr=" + seqNr + " d=" + data + " client="+client);
        // Send events of the network
        ModProtocol c = client;
        if (c != null) {
            ModProtocol.Package p = new ModProtocol.Package(cmdId, 0, seqNr, data);
            c.AddToSendQueue(p);
        }
    }
}
 