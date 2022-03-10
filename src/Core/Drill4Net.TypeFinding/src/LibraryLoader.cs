using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mono.Cecil;
using Drill4Net.Common;

namespace Drill4Net.TypeFinding
{
    public class LibraryLoader
    {
        public AssemblyDefinition LoadDefinition(string asmPath)
        {
            if (string.IsNullOrWhiteSpace(nameof(asmPath)))
                throw new ArgumentNullException(nameof(asmPath));

            //params
            var searches = new List<string> { Path.GetDirectoryName(asmPath) };
            var readerParams = new ReaderParameters
            {
                // we will write to another file, so we don't need this
                ReadWrite = false,
                // read everything at once
                ReadingMode = ReadingMode.Immediate,
                //exactly Source, not Destination (otherwise, overwriting is blocked - since dependency caching is currently used)
                AssemblyResolver = new AssemblyDefinitionResolver(searches),
            };

            //reading
            return AssemblyDefinition.ReadAssembly(asmPath, readerParams);
        }

        public Type LoadType(string asmPath, string fullName)
        {
            var asm  = LoadAssembly(asmPath);
             return asm.GetTypes().SingleOrDefault(a => a.FullName == fullName);
        }

        public Assembly LoadAssembly(string asmPath)
        {
            if (string.IsNullOrWhiteSpace(nameof(asmPath)))
                throw new ArgumentNullException(nameof(asmPath));
            //
            var asmName = Path.GetFileName(asmPath);
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            var asm = Array.Find(asms, a => a.ManifestModule.Name == asmName);
            if (asm == null)
                asm = Assembly.LoadFrom(asmPath);
            return asm;
        }
    }
}
