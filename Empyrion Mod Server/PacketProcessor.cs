using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empyrion_Mod_Server
{
    class PacketProcessor
    {
        #region  "Recieve Data from Game"
        //This function receives all data from the game
        private void packageReceivedDelegate(ModProtocol con, ModProtocol.Package p)
        {
            try
            {
                if (System.Windows.Application.Current == null) { return; }
                if (p.data == null)
                {
                    output(string.Format("Empty Package id rec: {0}", p.cmd));
                    return;
                }

                output(string.Format("Package id rec: {0}", p.cmd));

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
                                    output(string.Format("{0} Player with id {1}", i + 1, playerIds[i]));
                                }
                            }
                            else
                            {
                                output("No players connected");
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Info:
                        {
                            Eleon.Modding.PlayerInfo pInfo = (Eleon.Modding.PlayerInfo)p.data;
                            if (pInfo == null) { break; }
                            output(string.Format("Player info (seqnr {0}): cid={1} eid={2} name={3} playfield={4} fac={5}", p.seqNr, pInfo.clientId, pInfo.entityId, pInfo.playerName, pInfo.playfield, pInfo.factionId));

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
                            output(string.Format("Inventory received from player {0}", inv.playerId));
                            if (inv.toolbelt != null)
                            {
                                output("Toolbelt:");
                                for (int i = 0; inv.toolbelt != null && i < inv.toolbelt.Length; i++)
                                {
                                    output("  " + inv.toolbelt[i].slotIdx + ". " + inv.toolbelt[i].id + " " + inv.toolbelt[i].count + " " + inv.toolbelt[i].ammo);
                                }
                            }
                            if (inv.bag != null)
                            {
                                output("Bag:");
                                for (int i = 0; inv.bag != null && i < inv.bag.Length; i++)
                                {
                                    output("  " + inv.bag[i].slotIdx + ". " + inv.bag[i].id + " " + inv.bag[i].count + " " + inv.bag[i].ammo);
                                }
                            }
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Entity_PosAndRot:
                        {
                            Eleon.Modding.IdPositionRotation idPos = (Eleon.Modding.IdPositionRotation)p.data;
                            if (idPos == null) { break; }
                            output(string.Format("Player with id {0} position {1}, {2}, {3} rotation {4}, {5}, {6}", idPos.id, idPos.pos.x, idPos.pos.y, idPos.pos.z, idPos.rot.x, idPos.rot.y, idPos.rot.z));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Player_Credits:
                        {
                            Eleon.Modding.IdCredits idCredits = (Eleon.Modding.IdCredits)p.data;
                            if (idCredits == null) { break; }
                            output(string.Format("Credits player with id {0}: {1}", idCredits.id, idCredits.credits));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Ok:
                        {
                            output(string.Format("Event Ok seqnr {0}", p.seqNr));
                        }
                        break;

                    case Eleon.Modding.CmdId.Event_Error:
                        {
                            output(string.Format("Event Error seqnr {0}", p.seqNr));
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
                            output(string.Format("Global structures. Count: {0}", obj.globalStructures != null ? obj.globalStructures.Count : 0));

                            if (obj.globalStructures != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.structures.Clear()));

                                foreach (KeyValuePair<string, List<Eleon.Modding.GlobalStructureInfo>> kvp in obj.globalStructures)
                                {
                                    output(string.Format("Playfield {0}", kvp.Key));

                                    foreach (Eleon.Modding.GlobalStructureInfo g in kvp.Value)
                                    {
                                        StructureInfo stI = new StructureInfo();
                                        stI.FromStructureInfo(g, kvp.Key);

                                        output(string.Format("  id={0} name={1} type={2} #blocks={3} #devices={4} playfield={5} pos={6}/{7}/{8}", g.id, g.name, g.type, g.cntBlocks, g.cntDevices, kvp.Key, g.pos.x, g.pos.y, g.pos.z));

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
                            output(string.Format("Playfield list. Count: {0}", obj.playfields != null ? obj.playfields.Count : 0));
                            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayfields.Clear()));
                            foreach (string s in obj.playfields)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindowDataContext.onlinePlayfields.Add(s)));
                                output(string.Format("  {0}", s));
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

                            addEvent(string.Format("Event_Statistics: {0} {1} {2} {3}", obj.type, obj.int1, obj.int2, obj.int3));

                            //CoreRemoved,    int1: Structure id, int2: destryoing entity id, int3: (optional) controlling entity id
                            //CoreAdded,      int1: Structure id, int2: destryoing entity id, int3: (optional) controlling entity id
                            //PlayerDied,     int1: player entity id, int2: death type(Unknown = 0, Projectile = 1, Explosion = 2, Food = 3, Oxygen = 4, Disease = 5, Drowning = 6, Fall = 7, Suicide = 8), int3 :  (optional) other entity involved
                            //StructOnOff,    int1: structure id, int2: changing entity id, int3: 0 = off, 1 = on
                        }
                        break;

                    default:
                        output(string.Format("(1) Unknown package cmd {0}", p.cmd));
                        break;
                }
            }
            catch (Exception ex)
            {
                output(string.Format("Error: {0}", ex.Message));
            }
        }
        #endregion
    }
}
