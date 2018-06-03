using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class EmpyrionAPIMessageBroker
{
    private ModGameAPI GameAPI;
    private Queue<ushort> unusedSequenceNumbers;
    private Dictionary<ushort, Action<CmdId,object>> actionTracker = new Dictionary<ushort, Action<CmdId, object>>();


    public EmpyrionAPIMessageBroker(ModGameAPI dediAPI)
    {
        this.GameAPI = dediAPI;
        unusedSequenceNumbers = new Queue<ushort>(Enumerable.Range(60000, 60500).Select(x => (ushort)x));
    }

    private void defaultHandler(CmdId cmd, object any){}


    public void HandleCall(APICmd cmd)
    {
        this.HandleCall<Object>(cmd, defaultHandler);
    }

    public void HandleCall<ResponseType>(APICmd cmd, Action<ResponseType> handler)
    {
        Action<CmdId, object> outerHandler = (x, y) => handler((ResponseType)y);
        trackHandler(cmd, outerHandler);
    }

    public void HandleCall<ResponseType>(APICmd cmd, Action<CmdId, ResponseType> handler)
    {
        Action<CmdId, object> outerHandler = (x,y) => handler(x, (ResponseType)y);
        trackHandler(cmd, outerHandler);
    }

    private void trackHandler(APICmd cmd, Action<CmdId, object> handler)
    {
        var seqNr = unusedSequenceNumbers.Dequeue();
        actionTracker[seqNr] = handler;
        GameAPI.Game_Request(cmd.cmd, seqNr, cmd.data);
    }

    public void HandleMessage(CmdId eventId, ushort seqNr, object data)
    {
        if (!actionTracker.ContainsKey(seqNr)) return;
        var action = actionTracker[seqNr];
        action(eventId, data);
        deprovisionSequenceNumber(seqNr);

    }

    public void deprovisionSequenceNumber(ushort seqnr)
    {
        actionTracker.Remove(seqnr);
        unusedSequenceNumbers.Enqueue(seqnr);
    }

}

class APICmd
{

    public CmdId cmd { get; }
    public object data { get; }

    public APICmd(CmdId cmd, object data = null)
    {
        this.cmd = cmd;
        this.data = data;
    }

    public static APICmd operator + (APICmd cmd, object data)
    {
        return new APICmd(cmd.cmd, data);
    }

    public static implicit operator APICmd(CmdId d)
    {
        return new APICmd(d);
    }
}
