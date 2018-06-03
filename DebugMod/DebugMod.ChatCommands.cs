using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Eleon.Modding;
using System.Text.RegularExpressions;
using UnityEngine;
using DeJson;


public partial class DebugMod
{

    static Dictionary<String, Action<PString>> chatCommandActions = new Dictionary<String, Action<PString>>
    {
        { @"structures", x=>getStructures() },
        { @"rcc (.*)", execConsoleCommand  },
        { @"rex (.*)", remoteExecCommand },
        { @"rps (.*)", requestPlayfieldStats },
        { @"debugspawn", debugEntitySpawning },
        { @"testbroker", testBroker }
    };


    private static void getStructures()
    {
        GameAPI.Console_Write("Structures Requested");
        GameAPI.Game_Request(CmdId.Request_GlobalStructure_List, 0, null);
    }

    private static void execConsoleCommand(PString command)
    {
        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, 0, command);
        GameAPI.Console_Write($"text: {command.pstr}");
        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, 0, new Eleon.Modding.PString("SAY 'remote command'"));
    }

    private static void remoteExecCommand(PString pstr)
    {
        PString outCmd = new PString($"remoteex {pstr.pstr}");
        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, 0, outCmd);
        GameAPI.Console_Write($"remoteex: {outCmd.ToString()}");
        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, 0, new Eleon.Modding.PString("SAY 'remote exex command'"));
    }

    private static void requestPlayfieldStats(PString pstr)
    {
        GameAPI.Console_Write($"**** requesting stats for: {pstr.pstr}");
        GameAPI.Game_Request(CmdId.Request_Playfield_Stats, 0, pstr);
        GameAPI.Game_Request(CmdId.Request_ConsoleCommand, 0, new Eleon.Modding.PString("SAY 'requesting playfield stats'"));
    }

    private static void testBroker(PString pstr)
    {
        GameAPI.Console_Write($"**** executing broker test");
        var cmd = new APICmd(CmdId.Request_ConsoleCommand, new Eleon.Modding.PString("SAY 'broker test'"));

        
        broker.HandleCall<object>(cmd, (x,y)=> {
            switch (x)
            {
                case CmdId.Event_Ok:
                    GameAPI.Console_Write("test successful");
                    break;
                case CmdId.Event_Error:
                    GameAPI.Console_Write("test error");
                    break;
            }
        });
    }



    private static void debugEntitySpawning(PString prefabName)
    {
        EntitySpawnInfo info = new EntitySpawnInfo
        {

            playfield = "Akua",
            pos = new PVector3(11.5354891F, 58.5829468F, -11.8831434F),
            rot = new PVector3(0F, 139.371F, 0F),
            name = "BA_Alien",
            type = 2,
            factionGroup =2,
            factionId = 0,
            prefabName = "Infested-Test"
        };
        var cmd = new APICmd(CmdId.Request_Entity_Spawn, info);
        broker.HandleCall(cmd);
    }

    public static void Handle_chat_message(ChatInfo data)
    {

        AlertMessage($"chat message {data.type}");
        GameAPI.Console_Write($"chat message {data.type}");
        if (data.type != 3) return;
        var handled = false;


        foreach (var item in chatCommandActions)
        {
            var r = new Regex(item.Key);
            var match = r.Match(data.msg);

            if (!match.Success) continue;

            var content = match.Groups.Count > 0 ? match.Groups[1].ToString() : "";
            item.Value.Invoke(new PString(content));
            handled = true;

        }
    }
}