using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_CI, ConfiguratorConstants.COMMAND_NEW)]
    public class CiNewCommand : AbstractConfiguratorCommand
    {
        public CiNewCommand(ConfiguratorRepository rep) : base(rep)
        {
        }

        /************************************************************************/

        public override Task<bool> Process()
        {
            if (!ConfigureCiConfig(out var ciCfgPath))
                return Task.FromResult(false);
            return Task.FromResult(InjectCiToProjects(ciCfgPath));
        }

        private bool ConfigureCiConfig(out string ciCfgPath)
        {
            ciCfgPath = "";

            //asking
            if (!_cli.AskDirectory("Directory for the injections' configs (they will all be used)", out var dir, null, true, false))
                return false;
            int degree = 0;
            if (!_cli.AskDegreeOfParallelism("The degree of parallelism on level those configs", ref degree))
                return false;
            if (!_cli.AskFilePath("Test Runner's config path to run the injected targets", out var runCfgPath, null, true, false))
                return false;
            var defCfgPath = Path.Combine(dir, "ci.yml");
            if (!_cli.AskFilePath("Config path for this CI run will be saved to", out ciCfgPath, defCfgPath, false, true))
                return false;

            //setting
            var modelCfgPath = Path.Combine(_rep.Options.InstallDirectory, "ci.yml");
            CiOptions opts;
            if (File.Exists(modelCfgPath))
                opts = _rep.ReadCiOptions(modelCfgPath, false);
            else
                opts = new();

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
            var def = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"source\repos\");
            var dirExists = Directory.Exists(def);
            if (!dirExists)
                def = null;
            string dir;
            IList<string> projects;
            while (true)
            {
                if (!_cli.AskDirectory(@"This is CI integration questions. At the moment, integration is implemented only for IDEs (Visual Studio, Rider, etc). 
Please, specifiy the directory of one or more solutions with .NET source code projects", out dir, def, true, dirExists))
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
                    nums = answer.Split(',').Select(a => a.Trim()).Select(a => Convert.ToInt32(a)).ToArray();
                }
                catch
                {
                    RaiseWarning("Wrong input");
                    continue;
                }
                //
                if (nums.Min() < 1 || nums.Max() > projects.Count)
                {
                    RaiseWarning("Out of range, pleae repeat");
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
            return "Create new CI config in interactive mode (injections for the target + Test Runner's rules in CI pipeline)";
        }

        public override string GetHelp()
        {
            return "Help article not implemented yet";
        }
    }
}
