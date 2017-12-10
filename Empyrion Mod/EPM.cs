using System;
using System.Collections.Generic;
using Eleon.Modding;
using EPMConnector;

public class EPM : ModInterface {
    
    ModGameAPI GameAPI;

    Configuration config;

    List<ModProtocol.Package> receivedPackages = new List<ModProtocol.Package>();
    List<ModProtocol.Package> receivedPackagesTemp = new List<ModProtocol.Package>();
    ModTCPServer server;
    Type cmdType = typeof(Eleon.Modding.CmdId);

    public void output(string s)
    {
        GameAPI.Console_Write(s);
        
    }

    private void startModServer(ModGameAPI gameAPI)
    {
        var filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + "Settings.yaml";

        config = Configuration.GetConfiguration(filePath);

        server = new ModTCPServer(gameAPI);
        server.StartListen(config.GameServerIp, config.GameServerApiPort, PackageReceivedDelegate);
    }

    public void Game_Start(ModGameAPI gameAPI) {
        GameAPI = gameAPI;
        GameAPI.Console_Write("Mod Network Relay is Starting");
        startModServer(gameAPI);
        GameAPI.Console_Write("Mod Network Relay has been initialized");
    }

    // Called in a thread!
    private void PackageReceivedDelegate(ModProtocol con, ModProtocol.Package p) {
        GameAPI.Console_Write(string.Format("Package received, id: {0}, type: {1}", p.cmd, Enum.GetName(cmdType, p.cmd) ));

        lock (receivedPackages) {
            receivedPackages.Add(p);
        }
    }

    public void Game_Update() {
        
        // Exchange lists
       List <ModProtocol.Package> temp;
        lock (receivedPackages) {
            temp = receivedPackages;
            receivedPackages = receivedPackagesTemp;
            receivedPackagesTemp = temp;
            receivedPackages.Clear();
        }

        // Send request to the game
        ModProtocol.Package tmpPackage;
        if(temp.Count > 0)
        {
            GameAPI.Console_Write("Updating Game State");
        }

        for (int i=0; i<temp.Count; i++) {
            tmpPackage = temp[i];
            GameAPI.Console_Write(string.Format("updating game, id: {0}, type: {1}", tmpPackage.cmd, Enum.GetName(cmdType, tmpPackage.cmd)));
            GameAPI.Game_Request(tmpPackage.cmd, tmpPackage.seqNr, tmpPackage.data);
        }
        temp.Clear();
    }

    public void Game_Exit() {
        GameAPI.Console_Write("Mod Network Relay is Shutting Down");
        server.Close();
    }

    public void Game_Event(CmdId cmdId, ushort seqNr, object data) {
        GameAPI.Console_Write(string.Format("Game Generated package, id: {0}, type: {1}", cmdId, Enum.GetName(cmdType, cmdId)));
        server.SendRequest(cmdId, seqNr, data);
    }
}
 