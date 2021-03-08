﻿using System.Reflection;

/// <summary>
/// ALL THIS MUST BE INJECTED INTO THE PROFILED ASSEMBLY
/// </summary>
namespace Drill4Net.Injection
{
    /// <summary>
    /// This is the MODEL of proxy class that will be injected in the target assembly.
    /// It provides Reflection access to real profiling functionality with caching.
    /// </summary>
    public class ProfilerProxy
    {
        private static MethodInfo _methInfo; //not make it as readonly (cecilifier.me not understand it yet)

        /**************************************************************/

        static ProfilerProxy()
        {
            //hardcode or cfg?
            var profPath = @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Plugins.Test\bin\Debug\netstandard2.0\Plugins.Test.dll";
            var asm = Assembly.LoadFrom(profPath);
            var type = asm.GetType("Plugins.Test.LoggerPlugin");
            _methInfo = type.GetMethod("Process");
        }

        //cecilifier.me not understand static method yet
        public static void Process(string data)
        {
            _methInfo.Invoke(null, new object[] { data });
        }
    }
}