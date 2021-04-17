﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Profiling.Tree
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
            return Flatten(typeof(InjectedType)) //exactly for class, not assembly
                .Where(a => a.GetType().Name == nameof(InjectedDirectory))
                .Cast<InjectedDirectory>();
        }

        public IEnumerable<InjectedAssembly> GetAllAssemblies()
        {
            return Flatten(typeof(InjectedType))
                .Where(a => a.GetType().Name == nameof(InjectedDirectory))
                .Cast<InjectedAssembly>();
        }

        public IEnumerable<InjectedMethod> GetAllMethods()
        {
            return Flatten(typeof(CrossPoint))
                .Where(a => a.GetType().Name == nameof(InjectedMethod))
                .Cast<InjectedMethod>();
        }

        public IEnumerable<CrossPoint> GetAllPoints()
        {
            return Filter(typeof(InjectedMethod), true).Cast<CrossPoint>();
        }

        public InjectedDirectory GetDirectory(string path)
        {
            return Flatten(typeof(InjectedType))
                .FirstOrDefault(a => a.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase)) 
                as InjectedDirectory;
        }
    }
}
