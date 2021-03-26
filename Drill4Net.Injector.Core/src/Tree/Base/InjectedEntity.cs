using System;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedEntity : InjectedSimpleEntity
    {
        public string Fullname { get; set; }

        /****************************************************************/

        public InjectedEntity(string name) : base(name)
        {
        }

        public InjectedEntity(string name, string path) : base(name, path)
        {
        }
    }
}
