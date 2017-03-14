using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeathMessagesModule
{
    //TODO: When we get the telent / commands working, we can add more around the DatabaseManager.
    public class DeathMessages : ModInterface
    {
        ModGameAPI GameAPI;
        List<PlayerInfo> players;

        public void Game_Start(ModGameAPI dediAPI)
        {
            GameAPI = dediAPI;
            players = new List<PlayerInfo>();

            GameAPI.Console_Write("DM:  start");
        }

        private void NormalMessage(String msg)
        {
            GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 2, 10));
        }

        private void AlertMessage(String msg)
        {
            GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 1, 10));
        }

        private void AttentionMessage(String msg)
        {
            GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 0, 10));
        }

        public void Game_Event(CmdId eventId, ushort seqNr, object data)
        {
            try
            {
                switch (eventId)
                {
                    case CmdId.Event_Player_Info:
                        if (players.Where(e => e.entityId == ((PlayerInfo)data).entityId).Count() == 0)
                        {
                            players.Add((PlayerInfo)data);
                            GameAPI.Console_Write("DM: Adding player to list " + ((PlayerInfo)data).playerName);
                            //AlertMessage(((PlayerInfo)data).playerName + " joined");
                        }
                        break;
                    case CmdId.Event_Player_Connected:
                        //TODO: Add player to database, and set status to online.
                        GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, (Id)data);
                        GameAPI.Console_Write("DM: Connect " + ((Id)data).id.ToString());
                        break;
                    case CmdId.Event_Player_Disconnected:
                        //TODO: Update player status to offline.
                        players.Remove(players.FirstOrDefault(e => e.entityId == ((Id)data).id));
                        GameAPI.Console_Write("DM: Disconnect " + ((Id)data).id.ToString());
                        break;
                    case CmdId.Event_Statistics:

                        StatisticsParam stats = (StatisticsParam)data;

                        GameAPI.Console_Write(
                            String.Format("DM: Stats T - {0} | 1 - {1} | 2 - {2} | 3 - {3}", stats.type.ToString(), stats.int1.ToString(), stats.int2.ToString(), stats.int3.ToString())
                            );

                        if (stats.type == StatisticsType.PlayerDied)
                        {
                            String msg = String.Empty;

                            PlayerInfo result = players.FirstOrDefault(e => e.entityId == stats.int1);

                            String user = "Unknown";

                            if (result != null)
                                user = ((PlayerInfo)result).playerName;
                            else
                                GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Id(stats.int1));

                            switch (stats.int2)
                            {
                                case 1: // Projectile
                                    msg = String.Format("{0} bit a bullet!", user);
                                    break;
                                case 2: // Explosion
                                    msg = String.Format("{0} went up with a puff of smoke!", user);
                                    break;
                                case 3: // Food
                                    msg = String.Format("{0} took the Atkins diet too far!", user);
                                    break;
                                case 4: // Oxygen
                                    msg = String.Format("{0} took a deep breath and got nothing.", user);
                                    break;
                                case 5: // Disease
                                    msg = String.Format("{0} got a touch of the Ebola!", user);
                                    break;
                                case 6: // Drowning
                                    msg = String.Format("{0} tried to drink the lake and failed.", user);
                                    break;
                                case 7: // Fall
                                    msg = String.Format("{0} thought they could fly, but then gravity ruined that.", user);
                                    break;
                                case 8: // Suicide
                                    msg = String.Format("{0} helped darwinism by taking their own life.", user);
                                    break;
                                case 10:
                                    msg = String.Format("{0} became a tasty dinner for the wildlife.", user);
                                    break;
                                default:
                                    msg = String.Format("{0} isn't getting up from that.", user);
                                    break;
                            }

                            PlayerInfo killer = players.FirstOrDefault(e => e.entityId == stats.int3);

                            if (killer != null)
                                msg += String.Format("  Courtesy of {0}.", killer.playerName);

                            AlertMessage(msg);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                GameAPI.Console_Write(ex.Message);
            }
        }

        public void Game_Exit()
        {
            GameAPI.Console_Write("DM: Exit");
        }


        public void Game_Update()
        {
            
        }
    }
}
