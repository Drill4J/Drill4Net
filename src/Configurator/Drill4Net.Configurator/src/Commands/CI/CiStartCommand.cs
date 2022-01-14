using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI, ConfiguratorConstants.COMMAND_START)]
    public class CiStartCommand : AbstractConfiguratorCommand
    {
        public CiStartCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /******************************************************************/

        public override async Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return FalseEmptyResult;
            //
            var dir = _rep.GetCiDirectory();
            var res2 = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CI,
                dir, _desc, out var ciCfgPath,
                out var _, out var error);
            if (!res2)
            {
                RaiseError(error);
                return FalseEmptyResult;
            }
            if (string.IsNullOrWhiteSpace(ciCfgPath))
            {
                RaiseError("Path to the CI config was not found");
                return FalseEmptyResult;
            }
            //
            var opts = _rep.ReadCiOptions(ciCfgPath);
            var (res, err) = await StartCi(opts).ConfigureAwait(false);
            if (res)
            {
                const string mess = "CI workflow is done.";
                RaiseMessage(mess);
                _logger.Info(mess);
            }
            else
            {
                RaiseMessage(err);
                _logger.Error(err);
            }
            return TrueEmptyResult;
        }

        private async Task<(bool res, string error)> StartCi(CiOptions opts)
        {
            #region Checks
            if (opts == null)
                return (false, "The options' object is empty");

            var cfgsDir = opts.Injection?.ConfigDir;
            if (string.IsNullOrWhiteSpace(cfgsDir))
                return (false, $"The directory path of {CoreConstants.SUBSYSTEM_INJECTOR} configs is empty");
            if (!Directory.Exists(cfgsDir))
                return (false, $"The directory with {CoreConstants.SUBSYSTEM_INJECTOR} configs not found");

            var runCfgPath = opts.TestRunnerConfigPath;
            if (string.IsNullOrWhiteSpace(runCfgPath))
                return (false, $"The {CoreConstants.SUBSYSTEM_TEST_RUNNER} config's path is empty");
            if (!File.Exists(runCfgPath))
                return (false, $"The {CoreConstants.SUBSYSTEM_TEST_RUNNER} config not found");
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

            return (true, "");
        }

        private async Task<(bool res, string error)> InjectorProcess(string cfgsDir, int degreefParallelism)
        {
            var args = $"-{CoreConstants.ARGUMENT_SILENT} -{CoreConstants.ARGUMENT_DEGREE_PARALLELISM}={degreefParallelism} -{CoreConstants.ARGUMENT_CONFIG_DIR}=\"{cfgsDir}\"";
            var path = Path.Combine(_rep.GetInjectorDirectory(), "Drill4Net.Injector.App.exe");
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
            var path = Path.Combine(_rep.GetTestRunnerDirectory(), "Drill4Net.Agent.TestRunner.exe");
            var (res, pid) = CommonUtils.StartProgramm(CoreConstants.SUBSYSTEM_TEST_RUNNER, path, args, out var err);
            if (!res)
                return (false, err);

            //wait
            await CommonUtils.WaitForProcessExit(pid)
                .ConfigureAwait(false);
            return (true, "");
        }

        public override string GetShortDescription()
        {
            return "Start the CI pipeline.";
        }

        public override string GetHelp()
        {
            return @$"The command starts the CI pipeline: instrumentation by the {CoreConstants.SUBSYSTEM_INJECTOR} of one or more targets (SUT - system under test), and then launching automatic tests located in them using {CoreConstants.SUBSYSTEM_TEST_RUNNER}. The pipeline launch is currently integrated into the post-build event of .NET projects and is designed to facilitate development in the IDE. In the future, support for Jenkins, TeamCity, etc is planned.

{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_CI, RawContexts, "ci", true)}

The working folder for CI pipeline is installed in the config of the {CoreConstants.SUBSYSTEM_CONFIGURATOR} (for now – directly), but you can set the defaults by the command ""cfg restore"".
";
        }
    }
}
