using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DeathMessagesModule
{
    //TODO: When we get the telent / commands working, we can add more around the DatabaseManager.
    public class DeathMessages : ModInterface
    {
        ModGameAPI GameAPI;
        List<PlayerInfo> players;
        Config.Configuration config;

        public void Game_Start(ModGameAPI dediAPI)
        {
            GameAPI = dediAPI;
            players = new List<PlayerInfo>();

            GameAPI.Console_Write("Death Messages by joemorin73.");
            GameAPI.Console_Write("Part of the Empyrion Mod Sample collection.");

            var filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + "Messages.yaml";

            config = Config.Configuration.GetConfiguration(filePath);
        }

        private void ChatMessage(String msg)
        {
            String command = "SAY '" + msg + "'";
            GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_InGameMessage_AllPlayers, new Eleon.Modding.PString(command));
        }

        private void NormalMessage(String msg)
        {
            GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 0, 100));
        }

        private void AlertMessage(String msg)
        {
            GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 1, 100));
        }

        private void AttentionMessage(String msg)
        {
            GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 2, 100));
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

                            msg = String.Format(config.Messages.GetNextMessage(stats.int2), user);

                            PlayerInfo killer = players.FirstOrDefault(e => e.entityId == stats.int3);

                            if (killer != null)
                                msg += String.Format(config.Messages.GetNextMessage(-1), killer.playerName);

                            AlertMessage(msg);
                            if (config.MessageInChat)
                                ChatMessage(msg);

                        }
                        break;
                    case CmdId.Event_ChatMessage:
                        ChatInfo ci = (ChatInfo)data;
                        if (ci == null) { break; }

                        if (ci.type != 8 && ci.type != 7 && ci.msg == "!MODS")
                            ChatMessage("Death Messages by joemorin73.");
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