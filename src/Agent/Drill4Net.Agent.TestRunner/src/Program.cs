using System;
using System.Threading.Tasks;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Agent.TestRunner.Core;

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
                var cliParser = new CliParser(args, false);
                string cfgPath = args.Length > 0 ? args[0] : null;
                var rep = new TestRunnerRepository(cfgPath);
                _logger = new TypedLogger<Program>(rep.Subsystem);
                var runner = new Runner(rep);
                await runner.Run().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                Log.Flush();
                await Task.Delay(5000);
            }
            finally
            {
                _logger?.Info("Finished");
                _logger?.GetManager()?.Shutdown();
            }
        }
    }
}
