﻿using System;
using Drill4Net.Agent.Kafka.Transport;
using Drill4Net.Common;
using Serilog;

namespace Drill4Net.Agent.Kafka.Worker
{
    class Program
    {
        private static string _logPrefix;

        /***********************************************************/

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
                Console.WriteLine($"{_logPrefix}Worker has initialized");
                worker.Start();
            }
            catch (Exception ex)
            {
                var mess = ex.ToString();
                Log.Error(mess);
                Console.WriteLine($"{_logPrefix}Error:\n{mess}");
            }
        }

        private static void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            var mess = $"{_logPrefix}Local: {isLocal} -> {message}";
            if (isFatal)
                Log.Fatal(mess);
            else
                Log.Error(mess);
        }

        private static void Init()
        {
            _logPrefix = TransportUtils.GetLogPrefix(CoreConstants.SUBSYSTEM_PROBE_WORKER, typeof(Program));
            SetCaption();
        }

        private static void SetCaption()
        {
            //var name = typeof(ProbeWorker).Assembly.GetName().Name;
           // Console.Title = $"{name}: {Environment.ProcessId}";
        }
    }
}
