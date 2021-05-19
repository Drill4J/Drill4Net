using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class InjectedSolution : InjectedDirectory
    {
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public string Description { get; set; }

        /************************************************************************/

        public InjectedSolution(string path) : base(null, path)
        {
        }

        public InjectedSolution(string name, string path) : base(name, path)
        {
        }

        /************************************************************************/

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

        public InjectedDirectory GetFrameworkVersionRootDirectory(string moniker)
        {
            return _children.FirstOrDefault(a => a.Name == moniker) as InjectedDirectory;
        }

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
