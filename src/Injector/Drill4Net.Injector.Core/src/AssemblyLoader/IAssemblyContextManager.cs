using System.Reflection;

namespace Drill4Net.Injector.Core
{
    public interface IAssemblyContextManager
    {
        Assembly Load(string assemblyPath);
        void Unload(string assemblyPath);
    }
}