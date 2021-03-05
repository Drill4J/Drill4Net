using System;
using System.Reflection;

namespace Plugins.Demo
{
    class Program
    {
        public delegate void ProcDlg(string s);

        /**********************************************************************/

        static void Main(string[] args)
        {
            var path = @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Plugins.Logger\bin\Debug\netstandard2.0\Plugins.Logger.dll";
            var asm = Assembly.LoadFrom(path);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var type = asm.GetType("Plugins.Logger.LoggerPlugin");
            var methInfo = type.GetMethod("Process");
            //var parameters = new object[] { "YEAH..." };
            //methInfo.Invoke(null, parameters);

            //https://docs.microsoft.com/en-us/dotnet/api/system.delegate.createdelegate?view=net-5.0
            var dlg = (ProcDlg)Delegate.CreateDelegate(typeof(ProcDlg), null, methInfo); //static method
            dlg("DELEGATE");


            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //all dll including from NuGet must be replaced in plugin directory
            //in target's project add this in csproj file: 
            //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
            throw new Exception($"Dependency not found: [{args.Name}]");
        }
    }
}
