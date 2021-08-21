using System;

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
                worker.Start();
            }
            catch (Exception ex)
            {
                //TODO: log
                Console.WriteLine($"Error:\n{ex}");
            }
        }
    }
}
