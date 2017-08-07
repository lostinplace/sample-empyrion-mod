using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using ProtoBuf;
using Eleon.Modding;

public class ModProtocol {

	public struct Package {
		public CmdId cmd;
		public int clientId;
        public object data;
        public ushort seqNr;

        public Package(CmdId cmdId, int nClientId, ushort nSeqNr, object nData) {
			cmd = cmdId;
			clientId = nClientId;
            seqNr = nSeqNr;
            data = nData;
        }
    }

	public delegate void DelegatePackageReceived(ModProtocol con, Package p);
	private DelegatePackageReceived packageReceivedDelegate;

	public delegate void DelegateDisconnected(ModProtocol con);
	private DelegateDisconnected disconnectedDelegate;

	public readonly TcpClient tcpClient;

	private List<Package> writingPackages = new List<Package>();

	ModThreadHelper.Info readerThread;
	ModThreadHelper.Info writerThread;

	volatile BinaryReader readerStream;
    volatile BinaryWriter writerStream;

	AutoResetEvent evWritePackages = new AutoResetEvent(false);

	volatile bool bDisconnected = false;

	public ModProtocol(TcpClient nTcpClient, DelegatePackageReceived nPackageReceivedDelegate, DelegateDisconnected nDisconnectedDelegate) {
		tcpClient = nTcpClient;

		packageReceivedDelegate = nPackageReceivedDelegate;
		disconnectedDelegate = nDisconnectedDelegate;

		readerThread = ModThreadHelper.StartThread("Reader-"+tcpClient.Client.RemoteEndPoint, ReaderThread, ThreadPriority.Lowest);
		writerThread = ModThreadHelper.StartThread("Writer-"+tcpClient.Client.RemoteEndPoint, WriterThread, ThreadPriority.Lowest);
	}

	public void AddToSendQueue(Package p) {
		lock (writingPackages) {
			writingPackages.Add(p);
		}
		evWritePackages.Set();
	}

	private void ReaderThread(ModThreadHelper.Info ti) {

		readerStream = new BinaryReader(tcpClient.GetStream());

		try {
            while (!ti.eventRunning.WaitOne(0)) {
                CmdId cmd = (CmdId) readerStream.ReadByte();
                int clientId = readerStream.ReadInt32();
                ushort seqNr = readerStream.ReadUInt16();
				int len = readerStream.ReadInt32();
				int bytesRead=0; 
				byte[] data = null;
                if (len > 0) {
					data = new byte[len];				// allocations are bad! maybe use pooled buffers?
					do {
						bytesRead += readerStream.Read(data, bytesRead, (len - bytesRead));
                    } while (bytesRead < len && !readerThread.eventRunning.WaitOne(0));
				}

                object obj = null;
                if (len > 0) {
                    MemoryStream ms = new MemoryStream(data);
                    switch (cmd) {

                        case Eleon.Modding.CmdId.Event_Player_Connected:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Id>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_Disconnected:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Id>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_ChangedPlayfield:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdPlayfield>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_Inventory:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Inventory>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_List:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdList>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_Info:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PlayerInfo>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_Info:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Id>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_GetInventory:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Id>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_SetInventory:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Inventory>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_AddItem:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdItemStack>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_Credits:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Id>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_SetCredits:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdCredits>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_AddCredits:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdCredits>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_Credits:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdCredits>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Entity_PosAndRot:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Id>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Entity_PosAndRot:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdPositionRotation>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_InGameMessage_SinglePlayer:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdMsgPrio>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_InGameMessage_AllPlayers:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdMsgPrio>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_ShowDialog_SinglePlayer:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdMsgPrio>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_Loaded:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PlayfieldLoad>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_Unloaded:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PlayfieldLoad>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Playfield_Stats:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PString>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_Stats:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PlayfieldStats>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Dedi_Stats:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.DediStats>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_List:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PlayfieldList>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_GlobalStructure_List:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.GlobalStructureList>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Entity_Teleport:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdPositionRotation>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_GlobalStructure_Update:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PString>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Faction_Changed:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.FactionChangeInfo>(ms);
                            break;

                        case CmdId.Request_Blueprint_Finish:
                            obj = Serializer.Deserialize<Id>(ms);
                            break;

                        case CmdId.Request_Blueprint_Resources:
                            obj = Serializer.Deserialize<BlueprintResources>(ms);
                            break;

                        case CmdId.Request_Player_ChangePlayerfield:
                            obj = Serializer.Deserialize<IdPlayfieldPositionRotation>(ms);
                            break;

                        case CmdId.Request_Entity_ChangePlayfield:
                            obj = Serializer.Deserialize<IdPlayfieldPositionRotation>(ms);
                            break;

                        case CmdId.Request_Entity_Destroy:
                            obj = Serializer.Deserialize<Id>(ms);
                            break;

                        case CmdId.Request_Player_ItemExchange:
                        case CmdId.Event_Player_ItemExchange:
                            obj = Serializer.Deserialize<ItemExchangeInfo>(ms);
                            break;

                        case CmdId.Event_Statistics:
                            obj = Serializer.Deserialize<StatisticsParam>(ms);
                            break;

                        case CmdId.Event_Error:
                            obj = Serializer.Deserialize<ErrorInfo>(ms);
                            break;

                        case CmdId.Request_Structure_Touch:
                            obj = Serializer.Deserialize<Id>(ms);
                            break;

                        case CmdId.Request_Get_Factions:
                            obj = Serializer.Deserialize<Id>(ms);
                            break;

                        case CmdId.Event_Get_Factions:
                            obj = Serializer.Deserialize<FactionInfoList>(ms);
                            break;

                        case CmdId.Request_Player_SetPlayerInfo:
                            obj = Serializer.Deserialize<PlayerInfoSet>(ms);
                            break;

                        case CmdId.Event_NewEntityId:
                            obj = Serializer.Deserialize<Id>(ms);
                            break;

                        case CmdId.Request_Entity_Spawn:
                            obj = Serializer.Deserialize<EntitySpawnInfo>(ms);
                            break;
                            
                        case CmdId.Event_Player_DisconnectedWaiting:
                            obj = Serializer.Deserialize<Id>(ms);
                            break;

                        case CmdId.Event_ChatMessage:
                            obj = Serializer.Deserialize<ChatInfo>(ms);
                            break;

                        case CmdId.Request_ConsoleCommand:
                            obj = Serializer.Deserialize<PString>(ms);
                            break;

                        case CmdId.Request_Structure_BlockStatistics:
                            obj = Serializer.Deserialize<Id>(ms);
                            break;

                        case CmdId.Event_Structure_BlockStatistics:
                            obj = Serializer.Deserialize<IdStructureBlockInfo>(ms);
                            break;

                        case CmdId.Event_AlliancesAll:
                            obj = Serializer.Deserialize<AlliancesTable>(ms);
                            break;

                        case CmdId.Request_AlliancesFaction:
                            obj = Serializer.Deserialize<AlliancesFaction>(ms);
                            break;

                        case CmdId.Event_AlliancesFaction:
                            obj = Serializer.Deserialize<AlliancesFaction>(ms);
                            break;

                        case CmdId.Event_BannedPlayers:
                            obj = Serializer.Deserialize<BannedPlayerData>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_InGameMessage_Faction:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdMsgPrio>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_TraderNPCItemSold:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.TraderNPCItemSoldInfo>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_GetAndRemoveInventory:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Id>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_GetAndRemoveInventory:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.Inventory>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Playfield_Entity_List:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PString>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_Entity_List:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.PlayfieldEntityList>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Entity_Destroy2:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.IdPlayfield>(ms);
                            break;

                        case Eleon.Modding.CmdId.Request_Entity_Export:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.EntityExportInfo>(ms);
                            break;

                        case Eleon.Modding.CmdId.Event_ConsoleCommand:
                            obj = ProtoBuf.Serializer.Deserialize<Eleon.Modding.ConsoleCommandInfo>(ms);
                            break;
                    }
                }

                if (packageReceivedDelegate != null && bytesRead == len) {
					packageReceivedDelegate(this, new Package(cmd, clientId, seqNr, obj));
				}
            }
        }
        catch (IOException e) {
            Console.WriteLine(string.Format("Connection closed while reading ({0})", e.Message));
            ModLoging.Log(string.Format("MDP: Connection closed while reading ({0})", e.Message),ModLoging.eTyp.Exception);
            CloseConnection();
        }
        catch (ObjectDisposedException e) {
            Console.WriteLine(string.Format("ObjectDisposed: Connection closed while reading ({0})", e.Message));
            ModLoging.Log(string.Format("MDP: ObjectDisposed: Connection closed while reading ({0})", e.Message), ModLoging.eTyp.Exception);
            CloseConnection();
        }
        catch (Exception e) {
            Console.WriteLine(string.Format("Exception while reading ({0})", e.Message));
            ModLoging.Log(string.Format("MDP: Exception while reading ({0})", e.Message), ModLoging.eTyp.Exception);
            Console.WriteLine(e.GetType() + ": " + e.Message);
            CloseConnection();
        }
    }

	private void WriterThread(ModThreadHelper.Info ti) {

		writerStream = new BinaryWriter(tcpClient.GetStream());
		List<Package> writingPackagesCopy = new List<Package>();

		try {
			while (!ti.eventRunning.WaitOne(0)) {

				evWritePackages.WaitOne();

				// Do a copy of the list else we need to hold the lock when writing and this could block the main thread
				writingPackagesCopy.Clear();
				lock (writingPackages) {
					writingPackagesCopy.AddRange(writingPackages);
					writingPackages.Clear();
				}

				for (int i=0; i<writingPackagesCopy.Count; i++) {

                    Package p = writingPackagesCopy[i];
                    MemoryStream ms = new MemoryStream();
                    switch (p.cmd) {

                        case Eleon.Modding.CmdId.Event_Player_Connected:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_Disconnected:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_ChangedPlayfield:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdPlayfield>(ms, (Eleon.Modding.IdPlayfield)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_Inventory:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Inventory>(ms, (Eleon.Modding.Inventory)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_Info:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PlayerInfo>(ms, (Eleon.Modding.PlayerInfo)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_List:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdList>(ms, (Eleon.Modding.IdList)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_Info:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_GetInventory:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_SetInventory:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Inventory>(ms, (Eleon.Modding.Inventory)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_AddItem:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdItemStack>(ms, (Eleon.Modding.IdItemStack)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_Credits:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_SetCredits:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdCredits>(ms, (Eleon.Modding.IdCredits)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_AddCredits:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdCredits>(ms, (Eleon.Modding.IdCredits)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_Credits:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdCredits>(ms, (Eleon.Modding.IdCredits)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Entity_PosAndRot:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Entity_PosAndRot:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdPositionRotation>(ms, (Eleon.Modding.IdPositionRotation)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_InGameMessage_AllPlayers:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdMsgPrio>(ms, (Eleon.Modding.IdMsgPrio)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_InGameMessage_SinglePlayer:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdMsgPrio>(ms, (Eleon.Modding.IdMsgPrio)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_ShowDialog_SinglePlayer:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdMsgPrio>(ms, (Eleon.Modding.IdMsgPrio)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_Loaded:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PlayfieldLoad>(ms, (Eleon.Modding.PlayfieldLoad)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_Unloaded:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PlayfieldLoad>(ms, (Eleon.Modding.PlayfieldLoad)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Playfield_Stats:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PString>(ms, (Eleon.Modding.PString)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_Stats:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PlayfieldStats>(ms, (Eleon.Modding.PlayfieldStats)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Dedi_Stats:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.DediStats>(ms, (Eleon.Modding.DediStats)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_List:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PlayfieldList>(ms, (Eleon.Modding.PlayfieldList)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_GlobalStructure_List:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.GlobalStructureList>(ms, (Eleon.Modding.GlobalStructureList)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Entity_Teleport:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdPositionRotation>(ms, (Eleon.Modding.IdPositionRotation)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_GlobalStructure_Update:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PString>(ms, (Eleon.Modding.PString)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Faction_Changed:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.FactionChangeInfo>(ms, (Eleon.Modding.FactionChangeInfo)p.data);
                            break;

                        case CmdId.Request_Blueprint_Finish:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Id)p.data);
                            break;

                        case CmdId.Request_Blueprint_Resources:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.BlueprintResources>(ms, (BlueprintResources)p.data);
                            break;

                        case CmdId.Request_Player_ChangePlayerfield:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdPlayfieldPositionRotation>(ms, (Eleon.Modding.IdPlayfieldPositionRotation)p.data);
                            break;

                        case CmdId.Request_Entity_ChangePlayfield:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdPlayfieldPositionRotation>(ms, (Eleon.Modding.IdPlayfieldPositionRotation)p.data);
                            break;

                        case CmdId.Request_Entity_Destroy:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case CmdId.Request_Player_ItemExchange:
                        case CmdId.Event_Player_ItemExchange:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.ItemExchangeInfo>(ms, (Eleon.Modding.ItemExchangeInfo)p.data);
                            break;

                        case CmdId.Event_Statistics:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.StatisticsParam>(ms, (Eleon.Modding.StatisticsParam)p.data);
                            break;

                        case CmdId.Event_Error:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.ErrorInfo>(ms, (Eleon.Modding.ErrorInfo)p.data);
                            break;

                        case CmdId.Request_Structure_Touch:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case CmdId.Request_Get_Factions:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case CmdId.Event_Get_Factions:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.FactionInfoList>(ms, (Eleon.Modding.FactionInfoList)p.data);
                            break;

                        case CmdId.Request_Player_SetPlayerInfo:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PlayerInfoSet>(ms, (Eleon.Modding.PlayerInfoSet)p.data);
                            break;

                        case CmdId.Event_NewEntityId:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case CmdId.Request_Entity_Spawn:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.EntitySpawnInfo>(ms, (Eleon.Modding.EntitySpawnInfo)p.data);
                            break;
                            
                        case CmdId.Event_Player_DisconnectedWaiting:
                            Serializer.Serialize(ms, (Id)p.data);
                            break;

                        case CmdId.Event_ChatMessage:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.ChatInfo>(ms, (Eleon.Modding.ChatInfo)p.data);
                            break;

                        case CmdId.Request_ConsoleCommand:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PString>(ms, (Eleon.Modding.PString)p.data);
                            break;

                        case CmdId.Request_Structure_BlockStatistics:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case CmdId.Event_Structure_BlockStatistics:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdStructureBlockInfo>(ms, (Eleon.Modding.IdStructureBlockInfo)p.data);
                            break;

                        case CmdId.Event_AlliancesAll:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.AlliancesTable>(ms, (Eleon.Modding.AlliancesTable)p.data);
                            break;

                        case CmdId.Request_AlliancesFaction:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.AlliancesFaction>(ms, (Eleon.Modding.AlliancesFaction)p.data);
                            break;

                        case CmdId.Event_AlliancesFaction:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.AlliancesFaction>(ms, (Eleon.Modding.AlliancesFaction)p.data);
                            break;

                        case CmdId.Event_BannedPlayers:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.BannedPlayerData>(ms, (Eleon.Modding.BannedPlayerData)p.data);
                            break;
                            
                        case CmdId.Request_InGameMessage_Faction:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdMsgPrio>(ms, (Eleon.Modding.IdMsgPrio)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_TraderNPCItemSold:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.TraderNPCItemSoldInfo>(ms, (Eleon.Modding.TraderNPCItemSoldInfo)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Player_GetAndRemoveInventory:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Id>(ms, (Eleon.Modding.Id)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Player_GetAndRemoveInventory:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.Inventory>(ms, (Eleon.Modding.Inventory)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Playfield_Entity_List:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PString>(ms, (Eleon.Modding.PString)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_Playfield_Entity_List:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.PlayfieldEntityList>(ms, (Eleon.Modding.PlayfieldEntityList)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Entity_Destroy2:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.IdPlayfield>(ms, (Eleon.Modding.IdPlayfield)p.data);
                            break;

                        case Eleon.Modding.CmdId.Request_Entity_Export:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.EntityExportInfo>(ms, (Eleon.Modding.EntityExportInfo)p.data);
                            break;

                        case Eleon.Modding.CmdId.Event_ConsoleCommand:
                            ProtoBuf.Serializer.Serialize<Eleon.Modding.ConsoleCommandInfo>(ms, (Eleon.Modding.ConsoleCommandInfo)p.data);
                            break;

                        default:
                            break;
                    }

					writerStream.Write((byte) p.cmd);
					writerStream.Write((Int32) p.clientId);
                    writerStream.Write((UInt16) p.seqNr);
                    byte[] data = ms.GetBuffer();
                    int len = (int) ms.Length;
                    writerStream.Write((Int32) len);
                    if (len > 0) {
						writerStream.Write(data, 0, len);
					}
				}
				writerStream.Flush();
			}
		}
        catch (IOException) {
            //Console.WriteLine(string.Format("Connection closed while writing ({0})", e.Message));  silent
            CloseConnection();
        }
        catch (ObjectDisposedException e) {
            Console.WriteLine(string.Format("ObjectDisposed: Connection closed while writing ({0})", e.Message));
            ModLoging.Log(string.Format("MDP: ObjectDisposed: Connection closed while writing ({0})", e.Message), ModLoging.eTyp.Exception);
            CloseConnection();
        }
        catch (Exception e) {
            Console.WriteLine(string.Format("Connection closed while writing ({0})", e.Message));
            ModLoging.Log(string.Format("MDP: Connection closed while writing ({0})", e.Message), ModLoging.eTyp.Exception);
            Console.WriteLine(e.GetType() + ": " + e.Message);
            CloseConnection();
        }
    }

    void CloseConnection() {
        try
        {
            if (bDisconnected)
            {
                return;
            }
            bDisconnected = true;

            tcpClient.Close();

            readerStream.Close();
            readerThread.eventRunning.Set();
            writerStream.Close();

            writerThread.eventRunning.Set();
            evWritePackages.Set();

            if (disconnectedDelegate != null)
            {
                disconnectedDelegate(this);
            }
        }
        catch(Exception e)
        {
            ModLoging.Log_Exception(e,"MDP: Close Connection");
        }
        //readerThread.WaitForEnd();
        //writerThread.WaitForEnd();
    }

    public void Close() {
        try
        {
        tcpClient.Close();

        readerStream.Close();
        readerThread.eventRunning.Set();
        writerStream.Close();
        writerThread.eventRunning.Set();
        evWritePackages.Set();
        }
            catch(Exception e)
        {
            ModLoging.Log_Exception(e, "MDP: Close");
        }
    }
}
