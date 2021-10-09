using System;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.TestRunner.Core;
using System.Threading.Tasks;

namespace Drill4Net.Agent.TestRunner
{
    internal class Program
    {
        private static Logger _logger;

        /*******************************************************************/

        static async Task Main(string[] args)
        {
            AbstractRepository.PrepareEmergencyLogger();
            Log.Info("Starting...");

            try
            {
                var rep = new TestRunnerRepository();
                _logger = new TypedLogger<Program>(rep.Subsystem);
                var runner = new Runner(rep);
                await runner.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }

            _logger?.Info("Finished");
        }
    }
}
