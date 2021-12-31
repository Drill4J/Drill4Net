using System;
using System.IO;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute("CI")]
    public class CiCommand : AbstractCongifuratorCommand
    {
        public CiCommand(ConfiguratorRepository rep): base(rep)
        {
        }

        /******************************************************************/

        public override async Task<bool> Process()
        {
            var ciCfgPath = GetParamVal(ConfiguratorConstants.ARGUMENT_CONFIG_CI_PATH);
            if (ciCfgPath != null)
            {
                var opts = _rep.ReadCiOptions(ciCfgPath);
                var (res, err) = await StartCi(opts).ConfigureAwait(false);
                if (res)
                {
                    const string mess = "CI workflow is done.";
                    RaiseMessageDelivered(mess);
                    _logger.Info(mess);
                }
                else
                {
                    RaiseMessageDelivered(err);
                    _logger.Error(err);
                }
            }
            return true;
        }

        private async Task<(bool res, string error)> StartCi(CiOptions opts)
        {
            #region Checks
            if (opts == null)
                return (false, "The options' object is empty");

            var cfgsDir = opts.Injection?.ConfigDir;
            if (string.IsNullOrWhiteSpace(cfgsDir))
                return (false, "The directory of Injector's configs is empty");
            if (!Directory.Exists(cfgsDir))
                return (false, "The directory of Injector's configs not found");

            var runCfgPath = opts.TestRunnerConfigPath;
            if (string.IsNullOrWhiteSpace(runCfgPath))
                return (false, "The Test Runner config's path is empty");
            if (!File.Exists(runCfgPath))
                return (false, "The Test Runner config's not found");
            #endregion

            //degreeParallel
            int degreeParallel;
            if (opts.Injection?.DegreeOfParallelism == null)
                degreeParallel = Environment.ProcessorCount;
            else
                degreeParallel = Convert.ToInt32(opts.Injection.DegreeOfParallelism);

            // Injector
            var (res, err) = await InjectorProcess(cfgsDir, degreeParallel)
                .ConfigureAwait(false);
            if (!res)
                return (false, err);

            // Test Runner
            (res, err) = await TestRunnerProcess(runCfgPath)
                .ConfigureAwait(false);
            if (!res)
                return (false, err);

            return (true, null);
        }

        private async Task<(bool res, string error)> InjectorProcess(string cfgsDir, int degreefParallelism)
        {
            var args = $"-{CoreConstants.ARGUMENT_SILENT} -{CoreConstants.ARGUMENT_DEGREE_PARALLELISM}={degreefParallelism} -{CoreConstants.ARGUMENT_CONFIG_DIR}=\"{cfgsDir}\"";
            var path = Path.Combine(_rep.Options.InjectorDirectory, "Drill4Net.Injector.App.exe");
            var (res, pid) = CommonUtils.StartProgramm(CoreConstants.SUBSYSTEM_INJECTOR, path, args, out var err);
            if (!res)
                return (false, err);

            //wait
            await CommonUtils.WaitForProcessExit(pid);
            return (true, "");
        }

        private async Task<(bool res, string error)> TestRunnerProcess(string testRunnerCfgPath)
        {
            var args = $"-{CoreConstants.ARGUMENT_CONFIG_PATH}=\"{testRunnerCfgPath}\"";
            var path = Path.Combine(_rep.Options.TestRunnerDirectory, "Drill4Net.Agent.TestRunner.exe");
            var (res, pid) = CommonUtils.StartProgramm(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER, path, args, out var err);
            if (!res)
                return (false, err);

            //wait
            await CommonUtils.WaitForProcessExit(pid);
            return (true, "");
        }
    }
}
