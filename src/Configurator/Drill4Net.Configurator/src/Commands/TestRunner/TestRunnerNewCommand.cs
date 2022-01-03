using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_RUNNER, ConfiguratorConstants.COMMAND_NEW)]
    public class TestRunnerNewCommand : AbstractInteractiveCommand
    {
        public TestRunnerNewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /*******************************************************************************/

        public override Task<bool> Process()
        {
            _logger.Info("Start to Test Runner configure");

            RaiseMessage("\nDescribe the configuration of a specific tests: what and how Test Runner should be used.");

            var opts = _rep.Options;
            var modelCfgPath = Path.Combine(opts.InstallDirectory, "test_runner.yml");
            var cfg = _rep.ReadTestRunnerOptions(modelCfgPath);

            //desc
            if (!AskQuestion("Run's description", out string desc, null, false))
                return Task.FromResult(false);
            cfg.Description = desc;

            // DegreeOfParallelism
            if (!AskDegreeOfParallelism("Degree of parallelism (default)", out var degree))
                return Task.FromResult(false);
            cfg.DegreeOfParallelism = (byte)degree;

            // parallel restrict
            if (!AskQuestion("Does it need to limit the parallel execution of tests in this run by DEFAULT?", out string answer, "n"))
                return Task.FromResult(false);
            var runParalellRestrict = IsYes(answer);
            cfg.DefaultParallelRestrict = runParalellRestrict;

            const string asmHint = $@"Now you need to specify one or more tests' assemblies to run their tests. They can be located either in one folder or in several.
To finish, just enter ""{ConfiguratorConstants.COMMAND_OK}"".
Specify at least one tests' assembly.";
            RaiseMessage($"\n{asmHint}");

            if (cfg.Directories == null)
                cfg.Directories = new();
            if (!AddTestDirectories(cfg.Directories, runParalellRestrict, out string err))
                RaiseError(err);
            if (cfg.Directories.Count == 0)
                return Task.FromResult(false);

            // save the options
            var res = SaveConfig(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER, cfg, opts.TestRunnerDirectory ?? "");
            return Task.FromResult(res);
        }

        private bool AddTestDirectories(IList<RunDirectoryOptions> directories, bool runDefaultParalelRestrint, out string err)
        {
            err = "";
            while (true)
            {
                var question = directories.Count == 0 ? "Tests' directory" : "One more tests' directory";
                if (!AskDirectory(question, out var dir, null, false, false))
                    return false;
                if (IsOk(dir))
                    break;
                if (!AskQuestion("Does it need to limit the parallel execution of tests in this FOLDER by DEFAULT? It contains Xunit 2.x tests?",
                    out string answer, runDefaultParalelRestrint ? "y" : "n"))
                    return false;
                var dirParalellRestrict = IsYes(answer);
                //
                var dirRun = new RunDirectoryOptions
                {
                    Path = dir,
                    DefaultParallelRestrict = dirParalellRestrict,
                    Assemblies = new(),
                };
                //
                while (true)
                {
                    question = dirRun.Assemblies.Count == 0 ? "Tests' assembly name" : "One more tests' assembly name";
                    if (!AskFileName(question, out var asmName, null, false))
                        return false;
                    if (IsOk(asmName))
                        break;
                    if (!AskQuestion("Does it need to limit the parallel execution of tests in this ASSEMBLY? It contains Xunit 2.x tests?",
                        out answer, dirParalellRestrict ? "y" : "n"))
                        return false;
                    var asmParallelRestrict = IsYes(answer);

                    var asmRun = new RunAssemblyOptions
                    {
                        DefaultAssemblyName = asmName,
                        DefaultParallelRestrict = asmParallelRestrict,
                    };
                    dirRun.Assemblies.Add(asmRun);
                }

                directories.Add(dirRun);
            }
            return true;
        }

        public override string GetShortDescription()
        {
            return "";
        }

        public override string GetHelp()
        {
            return "Help article not implemeted yet";
        }
    }
}
