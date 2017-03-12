using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Eleon.Modding;

public class ModTCPServer {

	const string cIP = "127.0.0.1";
	const int cPort = 12345;

	TcpListener tcpListener;
	ModThreadHelper.Info listenThread;

	List<ModProtocol> clients = new List<ModProtocol>();

    ModProtocol.DelegatePackageReceived packageReceivedDelegate;
    ModGameAPI gameAPI;

    public ModTCPServer(ModGameAPI api) {
        gameAPI = api;
    }

	public void StartListen(ModProtocol.DelegatePackageReceived nPackageReceivedDelegate) {
        try
        {
            packageReceivedDelegate = nPackageReceivedDelegate;
            tcpListener = new TcpListener(IPAddress.Parse(cIP), cPort);
            listenThread = ModThreadHelper.StartThread(ListenForConnections, System.Threading.ThreadPriority.Lowest);
		        gameAPI.Console_Write("Now listening on port " + cPort);
        }
        catch(Exception e)
        {
            ModLoging.Log_Exception(e, "MTP: StartListen");
        }
	}

    public int GetClientCount() {
        lock (clients) {
            if (clients == null) return 0;
            return clients.Count;
        }
    }

    public void SendRequest(Eleon.Modding.CmdId cmdId, ushort seqNr, object data) {
        ModProtocol.Package p = new ModProtocol.Package(cmdId, 0, seqNr, data);
        lock (clients) {
            foreach (ModProtocol con in clients) {
                con.AddToSendQueue(p);
            }
        }
    }

	public void Close() {

        try
        {
            if (listenThread != null)
            {
                listenThread.WaitForEnd();
            }

            if (tcpListener != null)
            {
                ModLoging.Log( "MTP: TCP Listener was still open and will be closed",ModLoging.eTyp.Warning);
                closeTcpListener();
            }

            ModProtocol[] connectionArr;
            lock (clients)
            {
                connectionArr = clients.ToArray();      // need to copy this else the disconnected callback will remove it from list as well (and we get a thread lock)
            }
            foreach (ModProtocol con in connectionArr)
            {
                con.Close();
            }
        }
        catch(Exception e)
        {
            ModLoging.Log_Exception(e, "MTP: Close");
        }
	}

	private void ListenForConnections(ModThreadHelper.Info ti) {

        try
        {
            if (tcpListener == null) { return; };

            tcpListener.Start();

            while (!ti.eventRunning.WaitOne(0))
            {

                try
                {
                    // Step 0: Client connection
                    if (!tcpListener.Pending())
                    {
                        Thread.Sleep(100); // choose a number (in milliseconds) that makes sense
                        continue;
                    }

                    TcpClient tcpClient = tcpListener.AcceptTcpClient();        // blocks until a client connects

                    Console.WriteLine("New connection");

                    if (packageReceivedDelegate != null)
                    {
                        ModProtocol con = new ModProtocol(tcpClient, packageReceivedDelegate, disconnectedDelegate);

                        lock (clients)
                        {
                            clients.Add(con);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in ListenForConnections: " + e.Message);
                    ModLoging.Log_Exception(e, "MTP: ListenForConnections");
                    Console.WriteLine(e.Message);
                }
            }
            closeTcpListener();

        }
        catch(Exception e)
        {
            ModLoging.Log_Exception(e, "MTP: ListenForConnections Outer");
        }
	}

    private void closeTcpListener()
    {
        if (tcpListener != null) tcpListener.Stop();
        if (tcpListener.Server != null) { tcpListener.Server.Close(); };
        tcpListener = null;
    }

	// Called from thread!
	private void disconnectedDelegate(ModProtocol con) {

        // Remove from list
        if (clients != null)
        {
            lock (clients) {
                clients.Remove(con);
            }
        }
		// tbd
	}
 }


