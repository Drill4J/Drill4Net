using System;
using System.Linq;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// The metadata of the injected Target projects (directories, assemblies, 
    /// classes, methods, cross-points, etc)
    /// </summary>
    [Serializable]
    public class InjectedSolution : InjectedDirectory
    {
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public string SourceDir => Path;

        /// <summary>
        /// Specified product version from the injection configuration.
        /// </summary>
        public string ProductVersion { get; set; }
        public string Description { get; set; }

        /************************************************************************/

        public InjectedSolution(): base(null, null) { } //Serializable

        public InjectedSolution(string path) : this(null, path)
        {
        }

        public InjectedSolution(string name, string sourceDir) : base(sourceDir, sourceDir)
        {
            Name = name;
            Path = sourceDir;
        }

        /************************************************************************/

        /// <summary>
        /// Get list of the Tree entities' types
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetInjectedTreeTypes()
        {
            return new List<Type>
            {
                typeof(InjectedSolution),
                typeof(InjectedDirectory),
                typeof(InjectedAssembly),
                typeof(InjectedType),
                typeof(InjectedMethod),
                typeof(CrossPoint),
            };
        }

        /// <summary>
        /// Remove from the Tree empty entities
        /// </summary>
        public void RemoveEmpties()
        {
            var parents = CalcParentMap();

            CleanEntities(GetAllMethods().Cast<InjectedSimpleEntity>().ToList(), parents);
            CleanEntities(GetAllTypes().Cast<InjectedSimpleEntity>().ToList(), parents);
            CleanEntities(GetAllAssemblies().Cast<InjectedSimpleEntity>().ToList(), parents);
            CleanEntities(GetAllDirectories().Cast<InjectedSimpleEntity>().ToList(), parents);
        }

        /// <summary>
        /// If the ProductVersion property is not empty, it returns. Otherwise it tries 
        /// to get the version from processed assemblies.
        /// </summary>
        /// <returns></returns>
        public string GetProductVersion()
        {
            if(!string.IsNullOrWhiteSpace(ProductVersion))
                return ProductVersion;
            var asms = GetAssemblies().Where(a => a.ProductVersion != "0.0.0.0");
            var entryAsmVersion = asms.FirstOrDefault(a => a.HasEntryPoint)?.ProductVersion;
            if(entryAsmVersion != null)
                return entryAsmVersion;
            return asms.FirstOrDefault(a => !a.HasEntryPoint)?.ProductVersion;
        }
    }
}
