using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class DebugMod
{
    private static void ChatMessage(String msg)
    {
        String command = "SAY '" + msg + "'";
        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CmdId.Request_InGameMessage_AllPlayers, new Eleon.Modding.PString(command));
    }

    private static void NormalMessage(String msg)
    {
        GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 0, 100));
    }

    private static void AlertMessage(String msg)
    {
        GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 1, 100));
    }

    private static void AttentionMessage(String msg)
    {
        GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CmdId.Request_InGameMessage_AllPlayers, new IdMsgPrio(0, msg, 2, 100));
    }
}

