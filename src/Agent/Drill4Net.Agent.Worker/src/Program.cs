using System;
using Drill4Net.Common;
using System.Diagnostics;
using Drill4Net.Agent.Messaging;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            Init();
            StartWorker(args);
        }

        private static void StartWorker(string[] args)
        {
            try
            {
                var creator = new WorkerCreator(args);
                var worker = creator.CreateWorker();
                worker.ErrorOccured += Receiver_ErrorOccured;
                Log.Debug("Worker has initialized");
                worker.Start();
            }
            catch (Exception ex)
            {
                var mess = ex.ToString();
                Log.Error(mess);
            }
        }

        private static void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            var mess = $"Error (local: {isLocal}): {message}";
            if (isFatal)
                Log.Fatal(mess);
            else
                Log.Error(mess);
        }

        private static void Init()
        {
            //_logPrefix = MessagingUtils.GetLogPrefix(CoreConstants.SUBSYSTEM_AGENT_WORKER, typeof(Program));
            SetCaption();
        }

        private static void SetCaption()
        {
            if (Debugger.IsAttached)
            {
                var name = typeof(AgentWorker).Assembly.GetName().Name;
                Console.Title = $"{name}: {Environment.ProcessId}";
            }
        }
    }
}
