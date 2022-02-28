using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Diagnostics;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Assembly reader for an <see cref="AssemblyContext"/> creation 
    /// </summary>
    public class AssemblyReader
    {
        private readonly Logger _logger;

        /*****************************************************************************/

        public AssemblyReader()
        {
            _logger = new TypedLogger<AssemblyReader>(CoreConstants.SUBSYSTEM_INJECTOR);
        }

        /*****************************************************************************/

        /// <summary>
        /// Reads the assembly.
        /// </summary>
        /// <param name="runCtx">The Injector Engine's Run context.</param>
        /// <param name="searches">Search dirs for the resolving the dependencies.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">$"File not exists: [{filePath}]</exception>
        public AssemblyContext ReadAssembly(RunContext runCtx, List<string> searches = null)
        {
            var filePath = runCtx.ProcessingFile;
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
            var sDir = runCtx.Options.Source.Directory;
            if (searches == null || searches.Count == 0)
            {
                searches = new List<string> { sDir };
            }
            else
            {
                if(!searches.Contains(sDir))
                    searches.Add(sDir);
            }
            var readerParams = new ReaderParameters
            {
                // we will write to another file, so we don't need this
                ReadWrite = false,
                // read everything at once
                ReadingMode = ReadingMode.Immediate,
                //exactly Source, not Destination (otherwise, overwriting is blocked - since dependency caching is currently used)
                AssemblyResolver = new AssemblyDefinitionResolver(searches),
            };

            #region PDB
            var pdb = $"{asmCtx.SubjectName}.pdb";
            var isPdbExists = File.Exists(pdb);
            //TODO: +cfg? or by type of coverage/injection?
            var needPdb = isPdbExists && (version.FrameworkType is AssemblyVersionType.NetCore or AssemblyVersionType.NetStandard);
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
                        // _logger.Warning(ex, $"Reading PDB (from IDE): {nameof(ProcessAssembly)}");
                        //else
                        _logger.Error($"Reading PDB: {nameof(ReadAssembly)}", ex);
                }
                asmCtx.IsNeedPdb = needPdb;
            }
            #endregion
            #endregion

            //reading
            _logger.Debug($"Reading: [{filePath}]");
            asmCtx.Definition = AssemblyDefinition.ReadAssembly(filePath, readerParams);

            return asmCtx;
        }
    }
}
