using System;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using DeJson;



public partial class DebugMod: ModInterface
{
    static ModGameAPI GameAPI;
    static EmpyrionAPIMessageBroker broker;
    


    
    public void Game_Start(ModGameAPI dediAPI)
    {
        DebugMod.GameAPI = dediAPI;
        
        GameAPI.Console_Write("Debug Mod Launched! 12");
        unusedSettlementSequenceNumbers = new Queue<ushort>(Enumerable.Range(62000, 62500).Select(x => (ushort)x));
        broker = new EmpyrionAPIMessageBroker(dediAPI);
    }

    private void writeGlobalStructureList(GlobalStructureList data)
    {
        foreach(var kvp in data.globalStructures)
        {
            foreach(var y in kvp.Value)
            {
                string message = $"playfield: {kvp.Key}; struct: ${Serializer.Serialize(y)}";
                GameAPI.Console_Write(message);
                AlertMessage("sending to log");   
            }
        }
    }

    public void Game_Event(CmdId eventId, ushort seqNr, object data)
    {
        broker.HandleMessage(eventId, seqNr, data);

        if (settlementOperations.ContainsKey(seqNr))
        {
            var op = settlementOperations[seqNr];
            HandleSettlementWokflowEvent(GameAPI, op, eventId, data);
        }

        GameAPI.Console_Write($"ID:EVENT! {eventId} - {seqNr}");
        try
        {
            switch (eventId)
            {
                case CmdId.Event_Playfield_Stats:
                    var playfieldData = (PlayfieldStats)data;
                    var pstatstring = $"id: {Serializer.Serialize(playfieldData)}";
                    GameAPI.Console_Write(pstatstring);
                    break;
                case CmdId.Event_Player_Connected:
                    GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CmdId.Request_Player_Info, (Id)data);
                    break;
                case CmdId.Event_Statistics:

                    StatisticsParam stats = (StatisticsParam)data;
                    Handle_event_statistics(stats);
                    break;
                case CmdId.Event_ChatMessage:
                    ChatInfo ci = (ChatInfo)data;
                    Handle_chat_message(ci);
                    break;
                case CmdId.Event_GlobalStructure_List:
                    GlobalStructureList info = (GlobalStructureList)data;
                    writeGlobalStructureList(info);
                    break;
                default:
                    GameAPI.Console_Write($"event: {eventId}");
                    var outmessage = "NO DATA";
                    if (data != null)
                    {
                        outmessage = "data: " + data.ToString();
                    }
                    GameAPI.Console_Write(outmessage);
                    
                    break;
            }
        }
        catch (Exception ex)
        {
            GameAPI.Console_Write(ex.Message);
            GameAPI.Console_Write(ex.ToString());
        }
    }

    public void Game_Update()
    {
    }

    public void Game_Exit()
    {
    }
}