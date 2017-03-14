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
        Config.MessageCollection messages;

        public void Game_Start(ModGameAPI dediAPI)
        {
            GameAPI = dediAPI;
            players = new List<PlayerInfo>();

            GameAPI.Console_Write("Death Messages by joemorin73.");
            GameAPI.Console_Write("Part of the Empyrion Mod Sample collection.");

            var filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + "Messages.txt";

            messages = new Config.MessageCollection(filePath);
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
                        }
                        break;
                    case CmdId.Event_Player_Connected:
                        GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, (Id)data);
                        break;
                    case CmdId.Event_Player_Disconnected:
                        players.Remove(players.FirstOrDefault(e => e.entityId == ((Id)data).id));
                        break;
                    case CmdId.Event_Statistics:

                        StatisticsParam stats = (StatisticsParam)data;

                        if (stats.type == StatisticsType.PlayerDied)
                        {
                            String msg = String.Empty;

                            PlayerInfo result = players.FirstOrDefault(e => e.entityId == stats.int1);

                            String user = "Unknown";

                            if (result != null)
                                user = ((PlayerInfo)result).playerName;
                            else
                                GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, new Id(stats.int1));

                            msg = String.Format(messages.GetNextMessage(stats.int2), user);
                            
                            PlayerInfo killer = players.FirstOrDefault(e => e.entityId == stats.int3);

                            if (killer != null)
                                msg += String.Format(messages.GetNextMessage(-1), killer.playerName);

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

        public void Game_Update()
        {

        }

        public void Game_Exit()
        {
            GameAPI.Console_Write("DM: Exit");
        }
    }
}
