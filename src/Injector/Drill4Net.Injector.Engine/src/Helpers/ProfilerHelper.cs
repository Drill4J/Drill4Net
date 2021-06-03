using System.IO;
using Mono.Cecil;
using Drill4Net.Common;
using System.Reflection;
using System;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Helper for working with info for the Profiler assembly and class
    /// </summary>
    public static class ProfilerHelper
    {
        public static ModuleDefinition ProfilerModule { get; private set; }
        public static TypeReference TypeReference { get; private set; }
        private static Type _profilerType;

        public static MethodReference CreateProfilerReferences(ModuleDefinition targetModule, ProfilerOptions opts)
        {
            //MethodReference writeLine = module.ImportReference(typeof(Console).GetMethod("WriteLine"));

            if (ProfilerModule == null)
            {
                var profilerPath = Path.Combine(opts.Directory, opts.AssemblyName);
                ProfilerModule = ModuleDefinition.ReadModule(profilerPath, new ReaderParameters());
                TypeReference = targetModule.ImportReference(new TypeReference(opts.Namespace, opts.Class, ProfilerModule, ProfilerModule));

                var asm = Assembly.LoadFrom(profilerPath);
                _profilerType = asm.GetType($"{opts.Namespace}.{opts.Class}");
            }
            var methRef = targetModule.ImportReference(_profilerType.GetMethod(opts.Method));

            ////don't cache!
            //var methRef = targetModule.ImportReference(new MethodReference(opts.Method, ProfilerModule.TypeSystem.Void, TypeReference));
            //var strPar = new ParameterDefinition("data", ParameterAttributes.None, ProfilerModule.TypeSystem.String);
            //methRef.Parameters.Add(strPar);
            return methRef;
        }
    }
}
