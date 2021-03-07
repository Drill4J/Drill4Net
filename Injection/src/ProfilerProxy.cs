using System.Reflection;

/// <summary>
/// ALL THIS MUST BE INJECTED INTO THE PROFILED ASSEMBLY
/// </summary>
namespace Drill4J.Injection
{
    /// <summary>
    /// This is the proxy class that will be injected in the target assembly.
    /// It provides Reflection access to real profiling functionality with 
    /// some improvements such call to function through fast delegate
    /// </summary>
    public class ProfilerProxy
    {
        private static MethodInfo _methInfo; //not readonly

        /**************************************************************/

        static ProfilerProxy()
        {
            //hardcode or cfg?
            var profPath = @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Plugins.Logger\bin\Debug\netstandard2.0\Plugins.Logger.dll";
            var asm = Assembly.LoadFrom(profPath);
            var type = asm.GetType("Plugins.Logger.LoggerPlugin");
            _methInfo = type.GetMethod("Process");
        }

        public static void Process(string data)
        {
            _methInfo.Invoke(null, new object[] { data });
        }
    }
}
