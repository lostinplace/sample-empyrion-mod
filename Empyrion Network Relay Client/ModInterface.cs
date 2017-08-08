using ENRC.data;
using System;
using System.Collections.Generic;

namespace ENRC
{
    public partial class MainWindow
    {
        Type cmdType = typeof(Eleon.Modding.CmdId);

        static List<int> playerIds = new List<int>();
        static List<string> playfields = new List<string>();

        #region  "Send Requests to Game"        
        private void SendRequest(Eleon.Modding.CmdId cmdID, Eleon.Modding.CmdId seqNr, object data)
        {
            if (mainWindowDataContext != null && mainWindowDataContext.EnableOutput_SendRequest)
            {
                output(string.Format("SendRequest: Command {0} SeqNr: {1}", cmdID, seqNr), cmdID);
            }
            client.Send(cmdID, (ushort)seqNr, data);
        }

        private void Get_PlayerList()
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_List, Eleon.Modding.CmdId.Request_Player_List, null);
        }

        private void Get_PlayfieldList()
        {
            SendRequest(Eleon.Modding.CmdId.Request_Playfield_List, Eleon.Modding.CmdId.Request_Playfield_List, null);
        }

        private void GetPlayerInfo()
        {
            lock (playerIds)
            {
                foreach (int id in playerIds)
                {
                    SendRequest(Eleon.Modding.CmdId.Request_Player_Info, Eleon.Modding.CmdId.Request_Player_Info, new Eleon.Modding.Id(id));
                }
            }
        }

        private void GetAllPlayfieldStats()
        {
            lock (playfields)
            {
                foreach (string playfield in playfields)
                {
                    GetPlayfieldStats(playfield);
                }
            }
        }

        private void GetPlayfieldStats(string Playfield)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Playfield_Stats, Eleon.Modding.CmdId.Request_Playfield_Stats, new Eleon.Modding.PString(Playfield));
        }

        private void GetDediStats()
        {
            SendRequest(Eleon.Modding.CmdId.Request_Dedi_Stats, Eleon.Modding.CmdId.Request_Dedi_Stats, null);
        }

        private void Get_Strucutre_List()
        {
            SendRequest(Eleon.Modding.CmdId.Request_GlobalStructure_List, Eleon.Modding.CmdId.Request_GlobalStructure_List, null);
        }

        private void GetCredits(int entityId)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_Credits, Eleon.Modding.CmdId.Request_Player_Credits, new Eleon.Modding.Id(entityId));
        }

        private void GetCoordinates(int entityId)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Entity_PosAndRot, Eleon.Modding.CmdId.Request_Entity_PosAndRot, new Eleon.Modding.Id(entityId));
        }

        private void GetInventory(int entityId)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_GetInventory, Eleon.Modding.CmdId.Request_Player_GetInventory, new Eleon.Modding.Id(entityId));
        }

        private void GetAllStructureUpdates()
        {
            lock (playfields)
            {
                foreach (string playfield in playfields)
                {
                    GetStructureUpdate(playfield);
                }
            }
        }

        private void GetStructureUpdate(string Playfield)
        {
            SendRequest(Eleon.Modding.CmdId.Request_GlobalStructure_Update, Eleon.Modding.CmdId.Request_GlobalStructure_Update, new Eleon.Modding.PString(Playfield));
        }

        private void GetAllEntities()
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.entities.Clear()));

            lock (playfields)
            {
                foreach (string playfield in playfields)
                {
                    GetEntities(playfield);
                }
            }
        }

        private void GetEntities(string Playfield)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Playfield_Entity_List, Eleon.Modding.CmdId.Request_Playfield_Entity_List, new Eleon.Modding.PString(Playfield));
        }

        private void Get_Factions(int fromId = 1)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Get_Factions, Eleon.Modding.CmdId.Request_Get_Factions, new Eleon.Modding.Id(fromId));
        }

        private void Get_Structure_BlockStatistics(int entityId)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Structure_BlockStatistics, Eleon.Modding.CmdId.Request_Structure_BlockStatistics, new Eleon.Modding.Id(entityId));
        }

        private void Touch_Structure(int entityId)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Structure_Touch, Eleon.Modding.CmdId.Request_Structure_Touch, new Eleon.Modding.Id(entityId));
        }

        private void Get_Alliances()
        {
            SendRequest(Eleon.Modding.CmdId.Request_AlliancesAll, Eleon.Modding.CmdId.Request_AlliancesAll, null);
        }

        private void Entity_SetPosition(int entity_Id, Eleon.Modding.PVector3 co, Eleon.Modding.PVector3 rot)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Entity_Teleport, Eleon.Modding.CmdId.Request_Entity_Teleport, new Eleon.Modding.IdPositionRotation(entity_Id, co, rot));
        }

        private void Player_ChangePlayerfield(int entity_Id, string Playfield, Eleon.Modding.PVector3 co, Eleon.Modding.PVector3 rot)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_ChangePlayerfield, Eleon.Modding.CmdId.Request_Player_ChangePlayerfield, new Eleon.Modding.IdPlayfieldPositionRotation(entity_Id, Playfield, co, rot));
        }

        private void Entity_ChangePlayfield(int entity_Id, string Playfield, Eleon.Modding.PVector3 co, Eleon.Modding.PVector3 rot)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Entity_ChangePlayfield, Eleon.Modding.CmdId.Request_Entity_ChangePlayfield, new Eleon.Modding.IdPlayfieldPositionRotation(entity_Id, Playfield, co, rot));
        }

        private void Player_SetInventory(int entityId, Eleon.Modding.Inventory inventory)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_SetInventory, Eleon.Modding.CmdId.Request_Player_SetInventory, inventory);
        }

        private void Player_AddItem(int entityId, Eleon.Modding.ItemStack itemStack)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_AddItem, Eleon.Modding.CmdId.Request_Player_AddItem, new Eleon.Modding.IdItemStack(entityId, itemStack));
        }

        private void Player_AddCredits(int entityId, double credits)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_AddCredits, Eleon.Modding.CmdId.Request_Player_AddCredits, new Eleon.Modding.IdCredits(entityId, credits));
        }

        private void Player_SetCredits(int entityId, double credits)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_SetCredits, Eleon.Modding.CmdId.Request_Player_SetCredits, new Eleon.Modding.IdCredits(entityId, credits));
        }

        private void Player_SendMessage(int entityId, string message, byte prio = 2, float time = 10)
        {
            SendRequest(Eleon.Modding.CmdId.Request_InGameMessage_SinglePlayer, Eleon.Modding.CmdId.Request_InGameMessage_SinglePlayer, new Eleon.Modding.IdMsgPrio(entityId, message, prio, time));
        }

        private void SendMessage_All(string message, byte prio = 2, float time = 10)
        {
            SendRequest(Eleon.Modding.CmdId.Request_InGameMessage_AllPlayers, Eleon.Modding.CmdId.Request_InGameMessage_AllPlayers, new Eleon.Modding.IdMsgPrio(0, message, prio, time));
        }

        private void Player_ShowDialog_SinglePlayer(int entityId, string message, byte prio = 2, float time = 10)
        {
            SendRequest(Eleon.Modding.CmdId.Request_ShowDialog_SinglePlayer, Eleon.Modding.CmdId.Request_ShowDialog_SinglePlayer, new Eleon.Modding.IdMsgPrio(entityId, message, prio, time));
        }

        private void Blueprint_Finish(int entityId)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Blueprint_Finish, Eleon.Modding.CmdId.Request_Blueprint_Finish, new Eleon.Modding.Id(entityId));
        }

        private void Send_Command(string command)
        {
            SendRequest(Eleon.Modding.CmdId.Request_ConsoleCommand, Eleon.Modding.CmdId.Request_ConsoleCommand, new Eleon.Modding.PString(command));
        }

        private void EntitySpawn(int ID, string prefabName, string exportFile, string Playfield)
        {
            Eleon.Modding.EntitySpawnInfo spawnInfo;
            spawnInfo = new Eleon.Modding.EntitySpawnInfo();
            spawnInfo.forceEntityId = ID;
            spawnInfo.exportedEntityDat = exportFile;
            spawnInfo.prefabName = prefabName;
            spawnInfo.playfield = Playfield;

            SendRequest(Eleon.Modding.CmdId.Request_Entity_Spawn, Eleon.Modding.CmdId.Request_Entity_Spawn, spawnInfo);
        }

        private void Entity_Destroy(int entity_Id)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Entity_Destroy, Eleon.Modding.CmdId.Request_Entity_Destroy, new Eleon.Modding.Id(entity_Id));
        }

        private void Entity_Destroy2(int entity_Id, string playfield)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Entity_Destroy2, Eleon.Modding.CmdId.Request_Entity_Destroy2, new Eleon.Modding.IdPlayfield(entity_Id,playfield));
        }

        private void Request_Entity_Export(int entity_Id)
        {
          SendRequest(Eleon.Modding.CmdId.Request_Entity_Export, Eleon.Modding.CmdId.Request_Entity_Export, new Eleon.Modding.EntityExportInfo(entity_Id, null, true));
        }

        private void GetBannedPlayers()
        {
            SendRequest(Eleon.Modding.CmdId.Request_GetBannedPlayers, Eleon.Modding.CmdId.Request_GetBannedPlayers, null);
        }

        private void GetAndRemoveInventory(int entity_Id)
        {
            SendRequest(Eleon.Modding.CmdId.Request_Player_GetAndRemoveInventory, Eleon.Modding.CmdId.Request_Player_GetAndRemoveInventory, new Eleon.Modding.Id(entity_Id));
        }

        private void ItemExchange(int entity_Id)
        {
            Eleon.Modding.ItemStack[] itStack = new Eleon.Modding.ItemStack[] { new Eleon.Modding.ItemStack(2053, 1) };
            SendRequest(Eleon.Modding.CmdId.Request_Player_ItemExchange, Eleon.Modding.CmdId.Request_Player_ItemExchange, new Eleon.Modding.ItemExchangeInfo(entity_Id, "Player Item Exchange Title", "Put your description here", "Your button text here", itStack));
        }

        private void Request_NewID()
        {
             SendRequest(Eleon.Modding.CmdId.Request_NewEntityId, Eleon.Modding.CmdId.Request_NewEntityId, null);
        }
        #endregion

        #region  "Recieve Data from Game"        
        //This function receives all data from the game
        public void onGameEvent(ModProtocol.Package p)
        {
            try
            {
                if (System.Windows.Application.Current == null) { return; }
                if (p.data == null)
                {
                    output(string.Format("Empty Package id rec: {0}", p.cmd), p.cmd);
                    return;
                }

                if (mainWindowDataContext != null && mainWindowDataContext.EnableOutput_DataRecieved)
                {
                    output(string.Format("Package received, id: {0}, type: {1}", p.cmd, Enum.GetName(cmdType, p.cmd)));
                    output("received  event: c=" + p.cmd + " sNr=" + p.seqNr + " d=" + p.data + " client=" + client);
                }

                switch (p.cmd)
                {
                    case Eleon.Modding.CmdId.Event_Player_Connected:
                        {
                            int entityId = ((Eleon.Modding.Id)p.data).id;

                            addEvent(string.Format("Player with id {0} connected", entityId));

                            lock (playerIds)
                            {
                                playerIds.Add(entityId);
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayer.Add(entityId)));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Disconnected:
                        {
                            int entityId = ((Eleon.Modding.Id)p.data).id;

                            addEvent(string.Format("Player with id {0} disconnected", entityId));

                            lock (playerIds)
                            {
                                playerIds.Remove(entityId);
                            }
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayer.Remove(entityId)));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_List:
                        {
                            if (p.data != null)
                            {  // empyt list is null?!
                                lock (playerIds)
                                {
                                    playerIds = ((Eleon.Modding.IdList)p.data).list;
                                }
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayer.Clear()));
                                for (int i = 0; i < playerIds.Count; i++)
                                {
                                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayer.Add(playerIds[i])));
                                    output(string.Format("{0} Player with id {1}", i + 1, playerIds[i]), p.cmd);
                                }
                            }
                            else
                            {
                                output("No players connected", p.cmd);
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Info:
                        {
                            Eleon.Modding.PlayerInfo pInfo = (Eleon.Modding.PlayerInfo)p.data;
                            if (pInfo == null) { break; }
                            output(string.Format("Player info (seqnr {0}): cid={1} eid={2} name={3} playfield={4} fac={5}", p.seqNr, pInfo.clientId, pInfo.entityId, pInfo.playerName, pInfo.playfield, pInfo.factionId), p.cmd);

                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                            {
                                mainWindowDataContext.playerInfos.Clear();
                                PlayerInfo plI = new PlayerInfo();
                                plI.FromPlayerInfo(pInfo);
                                mainWindowDataContext.playerInfos.Add(plI);
                            }));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Inventory:
                        {
                            Eleon.Modding.Inventory inv = (Eleon.Modding.Inventory)p.data;
                            if (inv == null) { break; }
                            output(string.Format("Inventory received from player {0}", inv.playerId), p.cmd);
                            if (inv.toolbelt != null)
                            {
                                output("Toolbelt:", p.cmd);
                                for (int i = 0; inv.toolbelt != null && i < inv.toolbelt.Length; i++)
                                {
                                    output("  " + inv.toolbelt[i].slotIdx + ". " + inv.toolbelt[i].id + " " + inv.toolbelt[i].count + " " + inv.toolbelt[i].ammo, p.cmd);
                                }
                            }
                            if (inv.bag != null)
                            {
                                output("Bag:", p.cmd);
                                for (int i = 0; inv.bag != null && i < inv.bag.Length; i++)
                                {
                                    output("  " + inv.bag[i].slotIdx + ". " + inv.bag[i].id + " " + inv.bag[i].count + " " + inv.bag[i].ammo, p.cmd);
                                }
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Entity_PosAndRot:
                        {
                            Eleon.Modding.IdPositionRotation idPos = (Eleon.Modding.IdPositionRotation)p.data;
                            if (idPos == null) { break; }
                            output(string.Format("Player with id {0} position {1}, {2}, {3} rotation {4}, {5}, {6}", idPos.id, idPos.pos.x, idPos.pos.y, idPos.pos.z, idPos.rot.x, idPos.rot.y, idPos.rot.z), p.cmd);
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Credits:
                        {
                            Eleon.Modding.IdCredits idCredits = (Eleon.Modding.IdCredits)p.data;
                            if (idCredits == null) { break; }
                            output(string.Format("Credits player with id {0}: {1}", idCredits.id, idCredits.credits), p.cmd);
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Ok:
                        {
                            output(string.Format("Event Ok seqnr {0}", p.seqNr), p.cmd);
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Error:
                        {
                            Eleon.Modding.CmdId cmdId = (Eleon.Modding.CmdId)p.seqNr;
                            Eleon.Modding.ErrorInfo eInfo = (Eleon.Modding.ErrorInfo)p.data;

                            if (eInfo == null)
                            {
                                output(string.Format("Event Error seqnr {0}: TMD: p.data of Event_Error was not set", p.seqNr), p.cmd);
                            }
                            else
                            {
                                output(string.Format("Event Error {0} seqnr {1}", eInfo.errorType, cmdId), p.cmd);
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_ChangedPlayfield:
                        {
                            Eleon.Modding.IdPlayfield obj = (Eleon.Modding.IdPlayfield)p.data;

                            addEvent(string.Format("Player with id {0} changes to playfield {1}", obj.id, obj.playfield));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Playfield_Stats:
                        {
                            Eleon.Modding.PlayfieldStats obj = (Eleon.Modding.PlayfieldStats)p.data;
                            if (obj == null) { break; }
                            addStats(string.Format("Playfield stats for Akua: fps={0} heap={1} procid={2}", obj.fps, obj.mem, obj.processId));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Playfield_Loaded:
                        {
                            Eleon.Modding.PlayfieldLoad obj = (Eleon.Modding.PlayfieldLoad)p.data;
                            if (obj == null) { break; }

                            addEvent(string.Format("Playfield {0} loaded pid={1}", obj.playfield, obj.processId));

                            lock (playfields)
                            {
                                playfields.Add(obj.playfield);
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayfields.Add(obj.playfield)));
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Playfield_Unloaded:
                        {
                            Eleon.Modding.PlayfieldLoad obj = (Eleon.Modding.PlayfieldLoad)p.data;
                            if (obj == null) { break; }

                            addEvent(string.Format("Playfield {0} unloaded pid={1}", obj.playfield, obj.processId));

                            lock (playfields)
                            {
                                playfields.Remove(obj.playfield);
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayfields.Remove(obj.playfield)));
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Dedi_Stats:
                        {
                            Eleon.Modding.DediStats obj = (Eleon.Modding.DediStats)p.data;
                            if (obj == null) { break; }
                            addStats(string.Format("Dedi stats: {0}fps", obj.fps));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_GlobalStructure_List:
                        {
                            Eleon.Modding.GlobalStructureList obj = (Eleon.Modding.GlobalStructureList)p.data;
                            if (obj == null || obj.globalStructures == null) { break; }
                            output(string.Format("Global structures. Count: {0}", obj.globalStructures != null ? obj.globalStructures.Count : 0), p.cmd);

                            if (obj.globalStructures != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.structures.Clear()));

                                foreach (KeyValuePair<string, List<Eleon.Modding.GlobalStructureInfo>> kvp in obj.globalStructures)
                                {
                                    output(string.Format("Playfield {0}", kvp.Key), p.cmd);

                                    foreach (Eleon.Modding.GlobalStructureInfo g in kvp.Value)
                                    {
                                        StructureInfo stI = new StructureInfo();
                                        stI.FromStructureInfo(g, kvp.Key);

                                        output(string.Format("  id={0} name={1} type={2} #blocks={3} #devices={4} playfield={5} pos={6}/{7}/{8}", g.id, g.name, g.type, g.cntBlocks, g.cntDevices, kvp.Key, g.pos.x, g.pos.y, g.pos.z), p.cmd);

                                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.structures.Add(stI)));
                                    }
                                }
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Playfield_List:
                        {
                            Eleon.Modding.PlayfieldList obj = (Eleon.Modding.PlayfieldList)p.data;
                            if (obj == null || obj.playfields == null) { break; }
                            output(string.Format("Playfield list. Count: {0}", obj.playfields != null ? obj.playfields.Count : 0), p.cmd);
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayfields.Clear()));

                            lock (playfields)
                            {
                                playfields.Clear();
                                foreach (string s in obj.playfields)
                                {
                                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayfields.Add(s)));
                                    output(string.Format("  {0}", s), p.cmd);
                                    playfields.Add(s);
                                }
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Faction_Changed:
                        {
                            Eleon.Modding.FactionChangeInfo obj = (Eleon.Modding.FactionChangeInfo)p.data;
                            if (obj == null) { break; }

                            addEvent(string.Format("Faction changed entity: {0} faction id: {1} faction {2}", obj.id, obj.factionId, obj.factionGroup));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Statistics:
                        {
                            Eleon.Modding.StatisticsParam obj = (Eleon.Modding.StatisticsParam)p.data;
                            if (obj == null) { break; }

                            addEvent(string.Format("Event_Statistics: {0} {1} {2} {3} {4}", obj.type, obj.int1, obj.int2, obj.int3, obj.int4));

                            //CoreRemoved,    int1: Structure id, int2: destryoing entity id, int3: (optional) controlling entity id
                            //CoreAdded,      int1: Structure id, int2: destryoing entity id, int3: (optional) controlling entity id
                            //PlayerDied,     // int1: player entity id, int2: death type (Unknown = 0,Projectile = 1,Explosion = 2,Food = 3,Oxygen = 4,Disease = 5,Drowning = 6,Fall = 7,Suicide = 8), int3: (optional) other entity involved, int4: (optional) other entity CV/SV/HV id
                            //StructOnOff,    int1: structure id, int2: changing entity id, int3: 0 = off, 1 = on
                            //StructDestroyed,// int1: structure id, int2: type (0=wipe, 1=decay)
                        }
                        break;

                    case Eleon.Modding.CmdId.Request_ConsoleCommand:
                        {
                            Eleon.Modding.PString obj = (Eleon.Modding.PString)p.data;
                            if (obj == null) { break; }

                            output(string.Format("Request_ConsoleCommand: {0}", obj.pstr), p.cmd);
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_ChatMessage:
                        {
                            Eleon.Modding.ChatInfo obj = (Eleon.Modding.ChatInfo)p.data;
                            if (obj == null) { break; }

                            string typeName;
                            switch (obj.type)
                            {
                                case 7:
                                    typeName = "to faction";
                                    break;
                                case 8:
                                    typeName = "to player";
                                    break;
                                case 9:
                                    typeName = "to server";
                                    break;
                                default:
                                    typeName = "";
                                    break;
                            }

                            output(string.Format("Chat: Player: {0}, Recepient: {1}, Recepient Faction: {2}, {3}, Message: '{4}'", obj.playerId, obj.recipientEntityId, obj.recipientFactionId, typeName, obj.msg), p.cmd);
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_DisconnectedWaiting:
                        {
                            Eleon.Modding.Id obj = (Eleon.Modding.Id)p.data;
                            if (obj == null) { break; }

                            addEvent(string.Format("Event_Player_DisconnectedWaiting: Player: {0}", obj.id));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_AlliancesAll:
                        {
                            Eleon.Modding.AlliancesTable obj = (Eleon.Modding.AlliancesTable)p.data;
                            if (obj == null) { break; }

                            int facId1;
                            int facId2;

                            //Only differences to default alliances are listed (everyone in same Origin is by default allied)
                            foreach (int factionHash in obj.alliances)
                            {
                                facId1 = (factionHash >> 16) & 0xffff;
                                facId2 = factionHash & 0xffff;

                                output(string.Format("Alliance difference between faction {0} and faction {1}", facId1, facId2), p.cmd);
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Get_Factions:
                        {
                            Eleon.Modding.FactionInfoList obj = (Eleon.Modding.FactionInfoList)p.data;
                            if (obj == null || obj.factions == null) { break; }
                            output(string.Format("Faction list. Count: {0}", obj.factions != null ? obj.factions.Count : 0), p.cmd);
                            foreach (Eleon.Modding.FactionInfo fI in obj.factions)
                            {
                                output(string.Format("Id: {0}, Abrev: {1}, Name: {2}, Origin: {3}", fI.factionId, fI.abbrev, fI.name, fI.origin), p.cmd);
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Structure_BlockStatistics:
                        {
                            Eleon.Modding.IdStructureBlockInfo obj = (Eleon.Modding.IdStructureBlockInfo)p.data;
                            if (obj == null || obj.blockStatistics == null) { break; }

                            foreach (KeyValuePair<int, int> blockstat in obj.blockStatistics)
                            {
                                output(string.Format("Item {0}: Amount: {1}", blockstat.Key, blockstat.Value), p.cmd);
                            }
                            output(string.Format("Block statistic for {0}", obj.id), p.cmd);
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_BannedPlayers:
                        {
                            Eleon.Modding.BannedPlayerData obj = (Eleon.Modding.BannedPlayerData)p.data;
                            if (obj == null || obj.BannedPlayers == null) { break; }
                            output(string.Format("Banned list. Count: {0}", obj.BannedPlayers != null ? obj.BannedPlayers.Count : 0), p.cmd);
                            foreach (Eleon.Modding.BannedPlayerData.BanEntry ba in obj.BannedPlayers)
                            {
                                output(string.Format("Id: {0}, Date: {1}", ba.steam64Id, DateTime.FromBinary(ba.dateTime)), p.cmd);
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_TraderNPCItemSold:
                        {
                            Eleon.Modding.TraderNPCItemSoldInfo obj = (Eleon.Modding.TraderNPCItemSoldInfo)p.data;
                            if (obj == null) { break; }
                            output(string.Format("Trader NPC item sold info: TraderType: {0}, TraderId: {1}, PlayerId: {2}, StructureId: {3}, Item: {4}, Amount: {5}, Price: {6}", obj.traderType, obj.traderEntityId, obj.playerEntityId, obj.structEntityId, obj.boughtItemId, obj.boughtItemCount, obj.boughtItemPrice), p.cmd);
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_GetAndRemoveInventory:
                        {
                            Eleon.Modding.Inventory inv = (Eleon.Modding.Inventory)p.data;
                            if (inv == null) { break; }
                            output(string.Format("Got and removed Inventory from player {0}", inv.playerId), p.cmd);
                            if (inv.toolbelt != null)
                            {
                                output("Toolbelt:", p.cmd);
                                for (int i = 0; inv.toolbelt != null && i < inv.toolbelt.Length; i++)
                                {
                                    output("  " + inv.toolbelt[i].slotIdx + ". " + inv.toolbelt[i].id + " " + inv.toolbelt[i].count + " " + inv.toolbelt[i].ammo, p.cmd);
                                }
                            }
                            if (inv.bag != null)
                            {
                                output("Bag:", p.cmd);
                                for (int i = 0; inv.bag != null && i < inv.bag.Length; i++)
                                {
                                    output("  " + inv.bag[i].slotIdx + ". " + inv.bag[i].id + " " + inv.bag[i].count + " " + inv.bag[i].ammo, p.cmd);
                                }
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Playfield_Entity_List:
                        {
                            Eleon.Modding.PlayfieldEntityList obj = (Eleon.Modding.PlayfieldEntityList)p.data;
                            if (obj == null || obj.entities == null) { break; }
                            output(string.Format("Entities. Count: {0}", obj.entities != null ? obj.entities.Count : 0), p.cmd);

                            if (obj.entities != null)
                            {
                                output(string.Format("Playfield {0}", obj.playfield), p.cmd);

                                foreach (Eleon.Modding.EntityInfo g in obj.entities)
                                {
                                    EntityInfo stI = new EntityInfo();
                                    stI.FromEntityInfo(g, obj.playfield);

                                    output(string.Format("  id={0} type={1} playfield={2} pos={3}/{4}/{5}", g.id, g.type, obj.playfield, g.pos.x, g.pos.y, g.pos.z), p.cmd);

                                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.entities.Add(stI)));
                                }

                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_ConsoleCommand:
                        {
                            Eleon.Modding.ConsoleCommandInfo obj = (Eleon.Modding.ConsoleCommandInfo)p.data;
                            if (obj == null) { break; }
                            output(string.Format("Player {0}; Console command: {1} Allowed: {2}",obj.playerEntityId, obj.command, obj.allowed),p.cmd);
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_NewEntityId:
                        {
                            output(string.Format("New ID: {0}", ((Eleon.Modding.Id)p.data).id), p.cmd);
                            break;
                        }

                    default:
                        output(string.Format("(1) Unknown package cmd {0}", p.cmd), p.cmd);
                        break;
                }
            }
            catch (Exception ex)
            {
                output(string.Format("Error: {0}", ex.Message), p.cmd);
            }
        }
        #endregion

        private void output(string s)
        {
            output(s, Eleon.Modding.CmdId.Event_Ok);
        }

        private void output(string s, Eleon.Modding.CmdId cmdID)
        {
            Console.WriteLine(s);
            bool allowOutput = true;
            if (mainWindowDataContext != null && mainWindowDataContext.output != null && System.Windows.Application.Current != null)
            {
                switch (cmdID)
                {
                    case Eleon.Modding.CmdId.Event_Playfield_List:
                        allowOutput = mainWindowDataContext.EnableOutput_Event_Playfield_List;
                        break;

                    case Eleon.Modding.CmdId.Event_Playfield_Entity_List:
                    case Eleon.Modding.CmdId.Event_GlobalStructure_List:
                        allowOutput = mainWindowDataContext.EnableOutput_Event_GlobalStructure_List;
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Credits:
                        allowOutput = mainWindowDataContext.EnableOutput_Event_Player_Credits;
                        break;

                    case Eleon.Modding.CmdId.Event_Entity_PosAndRot:
                        allowOutput = mainWindowDataContext.EnableOutput_Event_Entity_PosAndRot;
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Inventory:
                        allowOutput = mainWindowDataContext.EnableOutput_Event_Player_Inventory;
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Info:
                        allowOutput = mainWindowDataContext.EnableOutput_Event_Player_Info;
                        break;

                    case Eleon.Modding.CmdId.Event_Player_List:
                        allowOutput = mainWindowDataContext.EnableOutput_Event_Player_List;
                        break;
                }
                if (allowOutput)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.output.Insert(0, s)));
                }
            }
        }

        private void addEvent(string s)
        {
            if (mainWindowDataContext != null && mainWindowDataContext.output != null && System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.events.Insert(0, s)));
            }
        }

        private void addStats(string s)
        {
            if (mainWindowDataContext != null && mainWindowDataContext.output != null && System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.stats.Insert(0, s)));
            }
        }
    }
}
