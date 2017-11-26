using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eleon.Modding;
using DeJson;



enum SettlementStage
{
    SiezedControl = 0,
    IdentifiedStructure = 1,
    IdentifiedReplacement = 2,
    ProvisionedReplacement = 3,
    RequestedDemolition = 4,
    DestroyedOriginal = 5,
    EmplacedNewSettlement = 6,
    ConfirmNewSettlement = 7,
    SettlementComplete = 8,

    SettlementInvalidated = 9
}

struct SettlementOperation
{
    public ushort seqNr;

    public int originalStructureId;
    public string originalStructureName;
    public GlobalStructureInfo originalStructureInfo;

    public string playfieldName;

    public Id newStructureId;
    public string newStructureName;
    public EntitySpawnInfo newStructureInfo;

    public SettlementStage stage;
}

public partial class DebugMod
{
    static Queue<ushort> unusedSettlementSequenceNumbers;
    static Dictionary<ushort, SettlementOperation> settlementOperations = new Dictionary<ushort, SettlementOperation>();


    private void Handle_event_statistics(StatisticsParam data)
    {
        if (data.type == StatisticsType.CoreRemoved)
        {
            var structureId = data.int1;
            var sequenceId = unusedSettlementSequenceNumbers.Dequeue();
            SettlementOperation operation = new SettlementOperation();
            operation.seqNr = sequenceId;
            operation.stage = 0;
            operation.originalStructureId = structureId;
            settlementOperations.Add(sequenceId, operation);
            GameAPI.Game_Request(CmdId.Request_GlobalStructure_List, sequenceId, null);
        }

    }

    private void HandleSettlementWokflowEvent(ModGameAPI GameAPI, SettlementOperation operation, CmdId eventType, object data)
    {

        var operationPayload = Serializer.Serialize(operation);

        var message = $"*** processing operation {operation.seqNr}\n *** cmdid:{eventType} \n *** " +
            $"last operation: {operation.stage} \n***  payload: {operationPayload}";
        GameAPI.Console_Write(message);


        switch (eventType)
        {
            case CmdId.Event_GlobalStructure_List:
                var structureList = (GlobalStructureList)data;
                operation = updateOperationFromStructureList(operation, structureList);
                if (operation.stage != SettlementStage.IdentifiedReplacement)
                {
                    deprovisionOperation(operation.seqNr);
                    return;
                }
                settlementOperations[operation.seqNr] = operation;
                GameAPI.Game_Request(CmdId.Request_NewEntityId, operation.seqNr, null);
                break;
            case CmdId.Event_NewEntityId:
                var newId = (Id)data;

                GameAPI.Console_Write($"*** new id: {Serializer.Serialize(newId)} ***");

                operation.newStructureId = newId;
                EntitySpawnInfo newInfo = new EntitySpawnInfo()
                {
                    forceEntityId = newId.id,
                    playfield = operation.playfieldName,
                    pos = operation.originalStructureInfo.pos,
                    rot = operation.originalStructureInfo.rot,
                    name = operation.newStructureName,
                    prefabName = "Test-Bed (Settled)",
                    type = operation.originalStructureInfo.type,
                };

                GameAPI.Console_Write($"*** requesting spawn: {Serializer.Serialize(newInfo)} ***");
                operation.newStructureInfo = newInfo;
                operation.stage = SettlementStage.ProvisionedReplacement;
                operation.stage = SettlementStage.RequestedDemolition;
                settlementOperations[operation.seqNr] = operation;
                Id outId = new Id(operation.originalStructureInfo.id);
                GameAPI.Game_Request(CmdId.Request_Entity_Destroy, operation.seqNr, outId);

                break;
            case CmdId.Event_Ok:
                if (operation.stage == SettlementStage.RequestedDemolition)
                {

                    operation.stage = SettlementStage.EmplacedNewSettlement;
                    settlementOperations[operation.seqNr] = operation;
                    GameAPI.Console_Write($"*** new settlement info:{Serializer.Serialize(operation.newStructureInfo)}");
                    GameAPI.Game_Request(CmdId.Request_Entity_Spawn, operation.seqNr, operation.newStructureInfo);
                }
                else if (operation.stage == SettlementStage.EmplacedNewSettlement)
                {
                    operation.stage = SettlementStage.SettlementComplete;
                    settlementOperations[operation.seqNr] = operation;
                    deprovisionOperation(operation.seqNr);
                    GameAPI.Console_Write("*** settlement complete!!! ***");

                }
                break;
            case CmdId.Event_Error:
                var error = (ErrorInfo)data;
                GameAPI.Console_Write($"*** error: {Serializer.Serialize(error)} ***");

                deprovisionOperation(operation.seqNr);
                break;
        }

    }

    private void deprovisionOperation(ushort seqnr)
    {
        settlementOperations.Remove(seqnr);
        unusedSettlementSequenceNumbers.Enqueue(seqnr);

    }


    private SettlementOperation updateOperationFromStructureList(SettlementOperation operation, GlobalStructureList globalStructureList)
    {
        foreach (var kvp in globalStructureList.globalStructures)
        {
            var structureList = kvp.Value;
            var applicableStructure = structureList.FirstOrDefault(x => x.id == operation.originalStructureId);

            if (object.Equals(applicableStructure, default(GlobalStructureInfo)))
                continue;
            operation.playfieldName = kvp.Key;
            operation.originalStructureInfo = applicableStructure;
            operation.originalStructureName = applicableStructure.name;
            if (!operation.originalStructureName.Contains("Infested"))
            {
                operation.stage = SettlementStage.SettlementInvalidated;
                return operation;
            }
            operation.newStructureName = operation.originalStructureName.Replace("Infested", "Settled");
            operation.stage = SettlementStage.IdentifiedReplacement;

            return operation;
        }

        return operation;
    }
}

