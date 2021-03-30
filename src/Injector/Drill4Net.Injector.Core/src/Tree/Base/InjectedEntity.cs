using System;
using System.Linq;

namespace Drill4Net.Injector.Core
{
    [Serializable]
    public class InjectedEntity : InjectedSimpleEntity
    {
        public string Fullname { get; set; }

        /****************************************************************/

        public InjectedEntity()
        {
        }

        public InjectedEntity(string name) : base(name)
        {
        }

        public InjectedEntity(string name, string path) : base(name, path)
        {
        }

        /****************************************************************/

        public InjectedSimpleEntity GetByFullname(string fullname)
        {
            return _children.Cast<InjectedEntity>().FirstOrDefault(a => a.Fullname == fullname);
        }
    }
}
