using System.Reflection;

namespace Drill4Net.Injection
{
    /// <summary>
    /// This is the MODEL of proxy class that must be injected into the target assembly.
    /// It provides Reflection access to real profiling functionality with caching.
    /// </summary>
    public class ProfilerProxy
    {
        private static MethodInfo _methInfo; //not make it as readonly (cecilifier.me not understand it yet)

        /**************************************************************/

        static ProfilerProxy()
        {
            //hardcode or cfg?
            var profPath = @"d:\Projects\EPM-D4J\!!_exp\Drill4Net\Drill4Net.Plugins.Test\bin\Debug\netstandard2.0\Drill4Net.Plugins.Test.dll";
            var asm = Assembly.LoadFrom(profPath);
            var type = asm.GetType("Drill4Net.Plugins.Test.LoggerPlugin");
            _methInfo = type.GetMethod("ProcessStatic");
        }

        //cecilifier.me not understand static method yet
        public static void Process(string data)
        {
            _methInfo.Invoke(null, new object[] { data });
        }
    }
}
