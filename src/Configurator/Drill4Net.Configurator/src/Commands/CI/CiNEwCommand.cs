using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI, ConfiguratorConstants.COMMAND_NEW)]
    public class CiNewCommand : AbstractConfiguratorCommand
    {
        public CiNewCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            if (_desc == null)
                return Task.FromResult(FalseEmptyResult);

            RaiseMessage("\nCreating a configuration for the CI pipeline.", CliMessageType.Info);

            var dir = _rep.GetCiDirectory();
            var res = _cmdHelper.GetSourceConfigPath<CiOptions>(CoreConstants.SUBSYSTEM_CI, dir, _desc,
                out var ciCfgPath, out var _, out var _);
            if (!res) //this is fine for a full setup
            {
                if (!ConfigureCiConfig(out ciCfgPath))
                    return Task.FromResult(FalseEmptyResult);
                if (!_cli.AskQuestion(@"At the moment, CI integration is implemented only for IDEs (Visual Studio, Rider, etc). Integration with the Jenkins, TeamCity, etc will be implement later.
So, do you want to integrate CI run into some source code projects (on its post-build events)?",
                    out var answer, "y"))
                    return Task.FromResult(FalseEmptyResult);
                if(!_cli.IsYes(answer))
                    return Task.FromResult(TrueEmptyResult);
            }
            var res2 = (InjectCiToProjects(ciCfgPath), new Dictionary<string, object>());
            return Task.FromResult(res2);
        }

        private bool ConfigureCiConfig(out string ciCfgPath)
        {
            ciCfgPath = "";

            //setting
            var modelCfgPath = Path.Combine(_rep.Options.InstallDirectory, "ci.yml");
            CiOptions opts;
            if (File.Exists(modelCfgPath))
                opts = _rep.ReadCiOptions(modelCfgPath, false);
            else
                opts = new();
            
            //description
            var def = opts.Description;
            if (!_cli.AskQuestion("CI run's description", out string desc, def, !string.IsNullOrWhiteSpace(def)))
                return false;
            opts.Description = desc;

            //asking
            if (!_cli.AskDirectory($"The Run have to has the one or more {CoreConstants.SUBSYSTEM_INJECTOR} configs. Specify the directory with them (they will all be used)", out var dir, null, true, false))
                return false;
            int degree = 0;
            if (!_cli.AskDegreeOfParallelism("The degree of parallelism on level those configs", ref degree))
                return false;
            if (!_cli.AskFilePath($"{CoreConstants.SUBSYSTEM_TEST_RUNNER} config path to run the injected targets", out var runCfgPath, null, true, false))
                return false;
            var defCfgPath = Path.Combine(dir, "ci.yml");
            if (!_cli.AskFilePath("Config path for this CI run will be saved to", out ciCfgPath, defCfgPath, false, true))
                return false;

            opts.Injection = new BatchInjectionOptions
            {
                ConfigDir = dir,
                DegreeOfParallelism = degree
            };
            opts.TestRunnerConfigPath = runCfgPath;

            //saving
            _rep.WriteCiOptions(opts, ciCfgPath);
            RaiseMessage("\nConfig was saved.");

            return true;
        }

        internal bool InjectCiToProjects(string ciCfgPath = "")
        {
            var ide = new IdeConfigurator(_rep);

            #region Search the projects
            var def = ide.GetDefaultProjectSourcesDirectory();
            string dir;
            IList<string> projects;
            while (true)
            {
                if (!_cli.AskDirectory(ConfiguratorConstants.MESSAGE_CI_INTEGRATION_IDE_DIR,
                    out dir, def, true, !string.IsNullOrWhiteSpace(def)))
                    return false;

                try
                {
                    projects = ide.GetProjects(dir);
                    var len = dir.Length;
                    projects = projects.Select(a => a.Substring(len)).OrderBy(x => x).ToList();
                    if (projects.Count == 0)
                    {
                        RaiseWarning("No projects detected.");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    var er = ex.ToString();
                    _logger.Error(er);
                    RaiseError($"Error: {er}");
                    continue;
                }
                break;
            }
            #endregion
            #region Select the projects
            RaiseMessage("\nThe found projects are:");
            for (int i = 0; i < projects.Count; i++)
            {
                string prj = projects[i];
                RaiseMessage($"{i + 1}. {prj}", CliMessageType.Info);
            }

            // select the projects
            int[] nums;
            var selected = new List<string>();
            while (true)
            {
                selected.Clear();
                if (!_cli.AskQuestion($"Select project numbers to inject CI operations into them (comma-separated digits from 1 to {projects.Count})", out var answer, null, false))
                    return false;
                if (string.IsNullOrWhiteSpace(answer))
                {
                    RaiseWarning("Input cannot be empty");
                    continue;
                }
                //
                try
                {
                    nums = answer.Split(',')
                        .Select(a => a.Trim())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .Select(a => Convert.ToInt32(a))
                        .OrderBy(a => a)
                        .ToArray();
                }
                catch
                {
                    RaiseWarning("Wrong input");
                    continue;
                }
                //
                if (nums.Min() < 1 || nums.Max() > projects.Count)
                {
                    RaiseWarning("Out of range, please repeat");
                    continue;
                }
                //
                foreach (var num in nums)
                    selected.Add($"{dir}{projects[num - 1]}");

                RaiseMessage("\nYou have selected these:");
                foreach (var prj in selected)
                {
                    RaiseMessage(prj, CliMessageType.Info);
                }
                if (!_cli.AskQuestion("Is that right?", out answer, "y"))
                    return false;
                if (_cli.IsYes(answer))
                    break;
            }
            #endregion
            #region Config
            if (string.IsNullOrWhiteSpace(ciCfgPath))
            {
                if (!_cli.AskFilePath("Config path for this CI run will read from", out ciCfgPath, null, true, false))
                    return false;
            }
            #endregion
            #region Injecting
            if (selected.Count > 0)
            {
                ide.InjectCI(selected, ciCfgPath, out var errors);
                if (errors.Count > 0)
                {
                    RaiseWarning("\nErrors occurred during processing:");
                    foreach (var error in errors)
                        RaiseError($"{error}");
                }
                var ending = selected.Count > 1 ? $"s: {selected.Count - errors.Count}/{selected.Count}" : null;
                RaiseMessage($"\nCI operation is created and injected to the project{ending}.", CliMessageType.Info);
            }
            #endregion
            return true;
        }

        public override string GetShortDescription()
        {
            return $"Create new {CoreConstants.SUBSYSTEM_CI} config in interactive mode (injections for the target + {CoreConstants.SUBSYSTEM_TEST_RUNNER} rules in CI pipeline).";
        }

        public override string GetHelp()
        {
            return $@"This command allows you:
  a). to create a configuration from scratch for the CI pipeline, connecting the injection stage for several independent configs and the stage of launching automatic tests by the {CoreConstants.SUBSYSTEM_TEST_RUNNER} contained in the corresponding targets (SUT - system under test). 
  b). to integrate the launch of the CI process by the generated config into the post-build event of compiling .NET projects to choose from, which allows you to fully automate the CI pipeline during the developing.

    Example: {RawContexts}

You can skip the stage of creating the config and configure only the injecting of CI procedures in source code projects using a named argument pointing to an already created config:
    Example: {RawContexts} --{CoreConstants.ARGUMENT_CONFIG_PATH}=""d:\configs\ci\cfg2.yml""

{HelpHelper.GetActiveLastSwitchesDesc(CoreConstants.SUBSYSTEM_CI, RawContexts)}";
        }
    }
}
