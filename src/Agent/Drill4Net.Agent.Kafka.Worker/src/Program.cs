using System;
using Serilog;

namespace Drill4Net.Agent.Kafka.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            SetCaption();
            StartWorker(args);
        }
        private static void StartWorker(string[] args)
        {
            try
            {
                var creator = new WorkerCreator(args);
                var worker = creator.CreateWorker();
                worker.ErrorOccured += Receiver_ErrorOccured;
                Console.WriteLine("Worker has initialized");
                worker.Start();
            }
            catch (Exception ex)
            {
                var mess = ex.ToString();
                Log.Error(mess);
                Console.WriteLine($"Error:\n{mess}");
            }
        }

        private static void Receiver_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            var mess = $"Local: {isLocal} -> {message}";
            if (isFatal)
                Log.Fatal(mess);
            else
                Log.Error(mess);
        }

        private static void SetCaption()
        {
            var name = typeof(CoverageWorker).Assembly.GetName().Name;
            Console.Title = $"{name}: {Environment.ProcessId}";
        }
    }
}
