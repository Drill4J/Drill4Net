using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Agent.TestRunner.Core;

namespace Drill4Net.Configurator
{
    public abstract class AbstractTestRunnerEditor : AbstractConfiguratorCommand
    {
        protected AbstractTestRunnerEditor(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**********************************************************************/

        public bool Edit(string cfgPath, bool isNew)
        {
            var appName = CoreConstants.SUBSYSTEM_TEST_RUNNER;
            _logger.Info($"Start configure the target: new={isNew}");

            RaiseMessage("\nDescribe the configuration of the specific tests: what and how Test Runner should be used.");

            var opts = _rep.Options;
            var cfg = _rep.ReadTestRunnerOptions(cfgPath);

            //desc
            var def = isNew ? null : cfg.Description;
            if (!_cli.AskQuestion("Run's description", out string desc, def, !string.IsNullOrWhiteSpace(def)))
                return false;
            cfg.Description = desc;

            // parallel restrict
            if (!_cli.AskQuestionBoolean("Does it need to limit the parallel execution of tests in this run by DEFAULT?", out var runParalellRestrict,
                isNew ? false : cfg.DefaultParallelRestrict))
                return false;
            cfg.DefaultParallelRestrict = runParalellRestrict;

            // DegreeOfParallelism
            var degree = isNew ? 0 : cfg.DegreeOfParallelism;
            if (!_cli.AskDegreeOfParallelism("The degree of parallelism potentially needed further in this configuration (DEFAULT)", ref degree))
                return false;
            cfg.DegreeOfParallelism = (byte)degree;

            #region Tests' assemblies
            const string asmHint = $@"Now you need to specify one or more tests' assemblies to run their tests. They can be located either in one folder or in several.
To finish, just enter ""{ConfiguratorConstants.ANSWER_OK}"".
Specify at least one tests' assembly.";
            RaiseMessage($"\n{asmHint}");

            if (cfg.Directories == null)
                cfg.Directories = new();

            var yes = true;
            if (!isNew && cfg.Directories.Count > 0)
            {
                if (!_cli.AskQuestionBoolean("Is it necessary to change the data on tests' assemblies? Existing records will be deleted.", out yes, false))
                    return false;
                if (yes)
                    cfg.Directories.Clear();
            }
            if (yes && !AddTestDirectories(cfg.Directories, runParalellRestrict, out string err))
                RaiseError(err);

            if (cfg.Directories.Count == 0)
            {
                RaiseWarning("No data about tests' assemblies. Need ot exit");
                return false;
            }
            #endregion

            // save the options
            if (isNew)
            {
                RaiseMessage($"\n{HelpHelper.GetInjectorAndRunnerConfigSavingNote(CoreConstants.SUBSYSTEM_TEST_RUNNER)}");
                return _cmdHelper.AskNameAndSave(CoreConstants.SUBSYSTEM_TEST_RUNNER, cfg, opts.TestRunnerDirectory ?? "", true);
            }
            else
            {
                return _cmdHelper.SaveConfig(appName, cfg, cfgPath);
            }
        }

        private bool AddTestDirectories(IList<RunDirectoryOptions> directories, bool runDefaultParalelRestrint, out string err)
        {
            err = "";
            while (true)
            {
                var question = directories.Count == 0 ? "Tests' directory" : "One more tests' directory";
                if (!_cli.AskDirectory(question, out var dir, null, false, false))
                    return false;
                if (_cli.IsOk(dir))
                    break;
                if (!_cli.AskQuestionBoolean("Does it need to limit the parallel execution of tests in this FOLDER by DEFAULT? Or does it contain Xunit 2.x tests?",
                    out var dirParalellRestrict, runDefaultParalelRestrint))
                    return false;
                //
                var dirRun = new RunDirectoryOptions
                {
                    Directory = dir,
                    DefaultParallelRestrict = dirParalellRestrict,
                    Assemblies = new(),
                };
                //
                while (true)
                {
                    question = dirRun.Assemblies.Count == 0 ? "Tests' assembly name" : "One more tests' assembly name";
                    if (!_cli.AskFileName(question, out var asmName, null, false))
                        return false;
                    if (_cli.IsOk(asmName))
                        break;
                    if (!_cli.AskQuestionBoolean("Does it need to limit the parallel execution of tests in this ASSEMBLY? Or does it contain Xunit 2.x tests?",
                        out var asmParallelRestrict, dirParalellRestrict))
                        return false;

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
    }
}
