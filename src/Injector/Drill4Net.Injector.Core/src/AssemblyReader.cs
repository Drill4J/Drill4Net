using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Drill4Net.Common;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Assembly reader for an <see cref="AssemblyContext"/> creation 
    /// </summary>
    public class AssemblyReader
    {
        /// <summary>
        /// Reads the assembly.
        /// </summary>
        /// <param name="runCtx">The Injector Engine's Run context.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">$"File not exists: [{filePath}]</exception>
        public AssemblyContext ReadAssembly(RunContext runCtx)
        {
            var filePath = runCtx.SourceFile;
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not exists: [{filePath}]");

            #region Version
            //need to know the version of the assembly before reading it via cecil

            AssemblyVersioning version;
            var versions = runCtx.Versions;
            if (versions.ContainsKey(filePath))
            {
                version = versions[filePath];
            }
            else
            {
                version = runCtx.Repository.TryGetAssemblyVersion(filePath);
                if (version != null)
                    versions.Add(filePath, version);
            }
            Console.WriteLine($"Version = {version}");
            #endregion

            var asmCtx = new AssemblyContext(runCtx.Options, filePath, version);
            if (asmCtx.Skipped)
                return asmCtx;

            #region Params
            var readerParams = new ReaderParameters
            {
                // we will write to another file, so we don't need this
                ReadWrite = false,
                // read everything at once
                ReadingMode = ReadingMode.Immediate,
                AssemblyResolver = new AssemblyResolver(),
            };

            #region PDB
            var pdb = $"{asmCtx.SubjectName}.pdb";
            var isPdbExists = File.Exists(pdb);
            //TODO: +cfg? or by type of coverage/injection?
            var needPdb = isPdbExists && (version.Target is AssemblyVersionType.NetCore or AssemblyVersionType.NetStandard);
            if (needPdb)
            {
                // netcore uses portable pdb, so we provide appropriate reader
                readerParams.SymbolReaderProvider = new PortablePdbReaderProvider();
                readerParams.ReadSymbols = true;
                try
                {
                    readerParams.SymbolStream = File.Open(pdb, FileMode.Open);
                }
                catch (IOException ex) //may be in VS for NET Core .exe
                {
                    if (!Debugger.IsAttached)
                        // Log.Warning(ex, $"Reading PDB (from IDE): {nameof(ProcessAssembly)}");
                        //else
                        Log.Error(ex, $"Reading PDB: {nameof(ReadAssembly)}");
                }
                asmCtx.IsNeedPdb = needPdb;
            }
            #endregion
            #endregion
            #region Reading
            // read subject assembly with symbols
            Log.Debug("Reading file [{FilePath}]", filePath);
#pragma warning disable DF0010 // Marks undisposed local variables.
            var assembly = AssemblyDefinition.ReadAssembly(filePath, readerParams);
 #pragma warning restore DF0010 // Marks undisposed local variables.
            asmCtx.Definition = assembly;
            #endregion

            return asmCtx;
        }
    }
}
