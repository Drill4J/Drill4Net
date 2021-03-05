using System;

namespace Plugins.Demo
{
    //all dll including from NuGet must be replaced in plugin directory
    //you need add this in csproj file of target project: 
    //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>

    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }

    }
}
