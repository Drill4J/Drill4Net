using System;
using System.Collections.Generic;
using System.Linq;

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
        public string Description { get; set; }

        /************************************************************************/

        public InjectedSolution(string path) : this(null, path)
        {
        }

        public InjectedSolution(string name, string path) : base(path, path)
        {
            Name = name;
            Path = path;
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
    }
}
