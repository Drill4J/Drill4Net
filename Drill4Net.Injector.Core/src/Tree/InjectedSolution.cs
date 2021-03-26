using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedSolution : InjectedSimpleEntity
    {
        public string DestinationPath { get; set; }
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

        public IEnumerable<InjectedDirectory> GetAllDirectories()
        {
            return Flatten(typeof(InjectedClass)) //exactly for class, not assembly
                .Where(a => a.GetType().Name == nameof(InjectedDirectory))
                .Cast<InjectedDirectory>();
        }

        public IEnumerable<InjectedAssembly> GetAllAssemblies()
        {
            return Flatten(typeof(InjectedClass))
                .Where(a => a.GetType().Name == nameof(InjectedDirectory))
                .Cast<InjectedAssembly>();
        }

        public IEnumerable<CrossPoint> GetAllPoints()
        {
            return Flatten()
                .Where(a => a.GetType().Name == nameof(CrossPoint))
                .Cast<CrossPoint>();
        }

        public InjectedDirectory GetDirectory(string path)
        {
            return Flatten(typeof(InjectedClass))
                .FirstOrDefault(a => a.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase)) 
                as InjectedDirectory;
        }
    }
}
