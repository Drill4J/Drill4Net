using System;
using System.Diagnostics;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Messaging.Transport;

namespace Drill4Net.Agent.Worker
{
    class Program
    {
        private static Logger _logger;

        /****************************************************************/

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
                _logger.Debug("Worker has initialized");
                worker.Start();
            }
            catch (Exception ex)
            {
                var mess = ex.ToString();
                _logger.Error(mess);
            }
        }

        private static void Receiver_ErrorOccured(IMessageReceiver source, bool isFatal, bool isLocal, string message)
        {
            var mess = $"Source = {source} -> error (local: {isLocal}): {message}";
            if (isFatal)
                _logger.Fatal(mess);
            else
                _logger.Error(mess);
        }

        private static void Init()
        {
            _logger = new TypedLogger<Program>(CoreConstants.SUBSYSTEM_AGENT_WORKER);
            SetCaption();
        }

        private static void SetCaption()
        {
            if (!Debugger.IsAttached)
                return;
            var name = typeof(AgentWorker).Assembly.GetName().Name;
            Console.Title = $"{name}: {Environment.ProcessId}";
        }
    }
}
