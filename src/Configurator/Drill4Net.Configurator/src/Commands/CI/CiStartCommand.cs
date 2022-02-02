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
                dir, _desc, out var ciCfgPath, out var _, out var error);
            if (!res2)
            {
                if (error != null)
                    RaiseError(error);
                return FalseEmptyResult;
            }
            if (string.IsNullOrWhiteSpace(ciCfgPath))
            {
                RaiseError("Path to the CI config is empty");
                return FalseEmptyResult;
            }
            //
            try
            {
                var opts = _rep.ReadCiOptions(ciCfgPath);
                var (res, err) = await StartCi(opts).ConfigureAwait(false);
                if (res)
                    RaiseMessage($"CI workflow is done: [{ciCfgPath}].");
                else
                    RaiseError(err);
            }
            catch (Exception ex)
            {
                RaiseError(ex.Message);
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
            (res, err) = await _cmdHelper.TestRunnerProcess(this, runCfgPath)
                .ConfigureAwait(false);
            if (!res)
                return (false, err);

            return (true, "");
        }

        private async Task<(bool res, string error)> InjectorProcess(string cfgsDir, int degreefParallelism)
        {
            var args = GetInjectorArguments(cfgsDir, degreefParallelism);
            var path = _rep.GetInjectorPath();
            var (res, pid) = CommonUtils.StartProgram(CoreConstants.SUBSYSTEM_INJECTOR, path, args, out var err);
            if (!res)
                return (false, err);

            //wait
            await CommonUtils.WaitForProcessExit(pid)
                .ConfigureAwait(false);
            return (true, "");
        }

        private string GetInjectorArguments(string cfgsDir, int degreefParallelism)
        {
            var args = $"--{CoreConstants.ARGUMENT_SILENT} --{CoreConstants.ARGUMENT_DEGREE_PARALLELISM}={degreefParallelism} --{CoreConstants.ARGUMENT_CONFIG_DIR}=\"{cfgsDir}\"";

            //overriding name
            var trgName = GetParameter(CoreConstants.ARGUMENT_TARGET_NAME);
            if (trgName != null)
                args += $" --{CoreConstants.ARGUMENT_TARGET_NAME}={trgName}";

            //overriding version
            var trgVersion = GetParameter(CoreConstants.ARGUMENT_TARGET_VERSION);
            if (trgVersion != null)
                args += $" --{CoreConstants.ARGUMENT_TARGET_VERSION}={trgVersion}";

            return args;
        }

        public override string GetShortDescription()
        {
            return "Start the CI pipeline.";
        }

        public override string GetHelp()
        {
            return @$"The command starts the CI pipeline: instrumentation by the {CoreConstants.SUBSYSTEM_INJECTOR} of one or more targets (SUT - system under test), and then launching automatic tests located in them using {CoreConstants.SUBSYSTEM_TEST_RUNNER}. The pipeline launch is currently integrated into the post-build event of .NET projects and is designed to facilitate development in the IDE. In the future, support for Jenkins, TeamCity, etc is planned.

{HelpHelper.GetArgumentsForSourceConfig(CoreConstants.SUBSYSTEM_CI, RawContexts, "ci", true)}

In a real CI/CD pipelines you should use --version option to specify version of the target(s), if you don't change the version in the Injector or agent configs directly (and automatically) each CI run.

    Example: {RawContexts} --{CoreConstants.ARGUMENT_TARGET_VERSION}=0.1.0

The working folder for CI pipeline is installed in the config of the {CoreConstants.SUBSYSTEM_CONFIGURATOR} (for now – directly), but you can set the defaults by the command ""cfg restore"".";
        }
    }
}
