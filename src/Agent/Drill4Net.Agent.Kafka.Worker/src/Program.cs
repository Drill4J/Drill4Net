using System;
using Serilog;

namespace Drill4Net.Agent.Kafka.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var creator = new WorkerCreator(args);
                var worker = creator.CreateWorker();
                worker.ErrorOccured += Receiver_ErrorOccured;
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
    }
}
