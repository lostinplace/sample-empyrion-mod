using System;
using System.Collections.Generic;
using System.Threading;

public class ModThreadHelper
{

    public class Info
    {
        public ThreadFunc threadDelegate;
        public string name;
        public Thread thread;
        public ManualResetEvent eventRunning = new ManualResetEvent(false);
        public void WaitForEnd()
        {
            eventRunning.Set();
        }
    }

    public delegate void ThreadFunc(Info ti);

    public static Dictionary<string, Info> RunningThreads = new Dictionary<string, Info>();

    public static Info StartThread(ThreadFunc nThreadStart, System.Threading.ThreadPriority nThreadPriority)
    {
        return StartThread(nThreadStart.Method.Name, nThreadStart, nThreadPriority);
    }

    public static Info StartThread(string nName, ThreadFunc nThreadFunc, System.Threading.ThreadPriority nThreadPrio)
    {

        Thread t = new Thread(new ParameterizedThreadStart(ThreadInvoke));
        t.Priority = nThreadPrio;

        Info threadInfo = new Info();
        threadInfo.threadDelegate = nThreadFunc;
        threadInfo.thread = t;

        lock (RunningThreads)
        {
            while (RunningThreads.ContainsKey(nName))
            {
                nName += "[new]";
            }
            threadInfo.name = nName;
            RunningThreads.Add(nName, threadInfo);
        }

        ThreadPool.UnsafeQueueUserWorkItem(ThreadInvoke, threadInfo);

        return threadInfo;
    }

    private static void ThreadInvoke(object ti)
    {

        Info threadInfo = (Info)ti;
        try
        {
            threadInfo.threadDelegate(threadInfo);
        }
        catch (Exception e)
        {
            Console.WriteLine(string.Format("Thread {0} exception:", threadInfo.name));
            Console.WriteLine(e.GetType() + ": " + e.Message);
            EPMConnector.ModLoging.Log(string.Format("MTH: Thread {0} exception: {1}", threadInfo.name, e.GetType() + ": " + e.Message));

        }
        finally
        {
            //Console.WriteLine(string.Format("Thread {0} exited", threadInfo.name));
            lock (RunningThreads)
            {
                RunningThreads.Remove(threadInfo.name);
            }
        }
    }
}
