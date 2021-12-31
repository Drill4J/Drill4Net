using System;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Agent.TestRunner
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            AbstractRepository.PrepareEmergencyLogger();
            Log.Info("Starting...");

            try
            {
                var cliDescriptor = new CliDescriptor(args, false);
                var rep = new TestRunnerRepository(cliDescriptor);
                var runner = new Runner(rep);
                await runner.Run().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }
            finally
            {
                Log.Info("Finished");
                Log.Shutdown();
                await Task.Delay(3000);
            }
        }
    }
}
