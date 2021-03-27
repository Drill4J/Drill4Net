using System;
using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedSimpleEntity : GenericTree<InjectedSimpleEntity>
    {
        public string Name { get; set; } 
        public string Path { get; set; }

        /*********************************************************************/

        public InjectedSimpleEntity(string name, string path = null)
        {
            Name = name;
            Path = path;
        }
    }
}
