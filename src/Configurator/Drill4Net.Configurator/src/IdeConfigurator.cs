using System;
using System.IO;
using System.Collections.Generic;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Configurator for CI operations by IDE
    /// </summary>
    public class IdeConfigurator
    {
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

        public void InjectCI(IEnumerable<string> paths, string ciCfgPath)
        {
            foreach (var path in paths)
            {
                
            }
        }
    }
}
