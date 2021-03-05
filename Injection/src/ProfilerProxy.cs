using System;
using System.Reflection;
                                            //ALL THIS MUST BE INJECTED INTO THE PROFILED ASSEMBLY

namespace Drill4J.Injection
{
    public static class ProfilerProxy
    {
        /// <summary>
        /// Delegate of injected method
        /// </summary>
        /// <param name="s"></param>
        public delegate void ProcDlg(string s);

        public static ProcDlg Process;

        /**********************************************************************/

        static ProfilerProxy()
        {
            //hardcode or cfg?
            var path = @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Plugins.Logger\bin\Debug\netstandard2.0\Plugins.Logger.dll";
            var asm = Assembly.LoadFrom(path);
            var type = asm.GetType("Plugins.Logger.LoggerPlugin");
            var methInfo = type.GetMethod("Process");
            Process = (ProcDlg)Delegate.CreateDelegate(typeof(ProcDlg), null, methInfo); //static method          
        }
    }
}
