﻿using System;
using System.IO;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Configurator for CI operations by IDE
    /// </summary>
    public class IdeConfigurator
    {
        private readonly ConfiguratorRepository _rep;
        private readonly Logger _logger;

        /*******************************************************************/

        public IdeConfigurator(ConfiguratorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<IdeConfigurator>(rep.Subsystem);
        }

        /*******************************************************************/

        public string GetDefaultProjectSourcesDirectory()
        {
            var prjDir = _rep.Options.ProjectsDirectory;
            var def = string.IsNullOrWhiteSpace(_rep.Options.ProjectsDirectory) || !Directory.Exists(prjDir) ?
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"source\repos\") :
                prjDir;
            var dirExists = Directory.Exists(def);
            if (!dirExists)
                def = "";
            return def ?? "";
        }

        public bool DeleteInjections(string prjDir, string ciCfgPath, out List<string> processed,
            out List<(string error, string path)> errors, out int all)
        {
            processed = new();
            errors = new();
            var projects = GetProjects(prjDir);
            all = projects.Count;
            var tag = GetAloneCommandTag(GetProjectCommand(ciCfgPath));
            foreach (var prjPath in projects)
            {
                if (!File.Exists(prjPath))
                    continue;
                var text = File.ReadAllText(prjPath);
                if (string.IsNullOrWhiteSpace(text))
                    continue;
                if (text.IndexOf(tag) == -1) //for counr projects with injected commands
                    continue;
                text = text.Replace(tag, null);

                //save
                try
                {
                    File.WriteAllText(prjPath, text);
                    processed.Add(prjPath);
                }
                catch (Exception ex)
                {
                    errors.Add((ex.ToString(), prjPath));
                }
            }
            return errors.Count == 0;
        }

        public List<string> GetProjectsWithCiIntegrations(string prjDir, string ciCfgPath)
        {
            var res = new List<string>();
            var tag = GetAloneCommandTag(GetProjectCommand(ciCfgPath));
            var projects = GetProjects(prjDir);
            foreach (var prjPath in projects)
            {
                if (!File.Exists(prjPath))
                    continue;
                var text = File.ReadAllText(prjPath);
                if (string.IsNullOrWhiteSpace(text))
                    continue;
                if (text.IndexOf(tag) == -1) //for count projects with injected commands
                    continue;
                res.Add(prjPath);
            }
            return res;
        }

        /// <summary>
        /// Find the .NET source code projects.
        /// </summary>
        /// <param name="root">The roor directory of some solutions</param>
        /// <returns>List of full paths the projects</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public IList<string> GetProjects(string root)
        {
            var projects = new List<string>();
            GetProjects(root, "*.csproj", ref projects);
            GetProjects(root, "*.vbproj", ref projects); //TODO: add then *.fsproj
            return projects;
        }

        /// <summary>
        /// Find the .NET source code projects.
        /// </summary>
        /// <param name="dir">The roor directory of some solutions.</param>
        /// <param name="mask">The mask for search the projects</param>
        /// <param name="projects">List of full paths the projects</param>
        /// <returns>L</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        internal void GetProjects(string dir, string mask, ref List<string> projects)
        {
            if (string.IsNullOrWhiteSpace(mask))
                throw new ArgumentNullException(nameof(mask));
            if(!Directory.Exists(dir))
                throw new DirectoryNotFoundException($"Root directory to search the projects not found: {dir}");
            if (projects == null)
                throw new ArgumentNullException(nameof(projects));
            //
            var files = Directory.GetFiles(dir, mask);
            foreach (var file in files)
                projects.Add(file);

            var dirs = Directory.GetDirectories(dir);
            foreach (var curDir in dirs)
                GetProjects(curDir, mask, ref projects);
        }

        public void InjectCI(IEnumerable<string> paths, string ciCfgPath, out List<string> errors)
        {
            if(string.IsNullOrWhiteSpace(ciCfgPath))
                throw new ArgumentNullException(nameof(ciCfgPath));
            //
            errors = new();
            var command = GetProjectCommand(ciCfgPath);
            foreach (var path in paths)
            {
                var res = InjectCiCommandTo(path, command, out var error);
                if (!res)
                {
                    errors.Add(error);
                    _logger.Error(error);
                }
                else
                {
                    _logger.Info($"CI Command injected into the project file: [{path}]");
                }
            }
        }

        internal bool InjectCiCommandTo(string prjPath, string command, out string error)
        {
            #region Checks
            error = "";
            if (string.IsNullOrWhiteSpace(prjPath))
            {
                error = "The project path is empty";
                return false;
            }
            if (string.IsNullOrWhiteSpace(command))
            {
                error = "The command for the injecting is empty";
                return false;
            }
            if(!File.Exists(prjPath))
            {
                error = $"The project file does not exist: [{prjPath}]";
                return false;
            }
            #endregion

            var text = File.ReadAllText(prjPath);
            if (string.IsNullOrWhiteSpace(text))
            {
                error = $"The project file is empty in [{prjPath}]";
                return false;
            }

            //already injected?
            if(text.IndexOf(command) != -1)
                return true;

            //TODO: use $MS API for that???

            //search
            const string search = @"<Target Name=""PostBuild"" AfterTargets=""PostBuildEvent"">";
            //but changed in-place spaces are not taken into account here
            var ind = text.IndexOf(search, StringComparison.InvariantCultureIgnoreCase);
            if (ind > -1)
            {
                //we have to inject our event as last one
                const string lastTag = "</Target>";
                var ind2 = text.IndexOf(lastTag, ind, StringComparison.InvariantCultureIgnoreCase);
                if (ind2 == -1)
                {
                    error = $"The project structure for PostBuild is present, but incorrect: [{prjPath}]";
                    return false;
                }
                ind2--;
                text = text.Insert(ind2, $"    {GetAloneCommandTag(command)}\n");
            }
            else
            {
                var fullBlock = $@"
  <Target Name=""PostBuild"" AfterTargets=""PostBuildEvent"">
    <Exec Command=""{command}"" />
  </Target>";
                var ind2 = text.IndexOf("</Project>", 100, StringComparison.InvariantCultureIgnoreCase);
                if (ind2 == -1)
                {
                    error = $"The project structure for PostBuild is incorrect: [{prjPath}]";
                    return false;
                }
                ind2--;
                text = text.Insert(ind2, $"{fullBlock}\n");
            }

            //save
            try
            {
                File.WriteAllText(prjPath, text);
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }

            return true;
        }
        
        internal string GetPureCommand(string ciCfgPath)
        {
            return @$"""{_rep.GetAppPath()}"" ci start --{CoreConstants.ARGUMENT_CONFIG_PATH}=""{ciCfgPath}""";
        }

        internal string GetProjectCommand(string ciCfgPath)
        {
            var command = GetPureCommand(ciCfgPath);
            return command.Replace(@"""", "&quot;");
        }

        internal string GetAloneCommandTag(string command)
        {
            return $"<Exec Command=\"{command}\" />";
        }
    }
}
