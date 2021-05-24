using System;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Serilog;
using Drill4Net.Common;

namespace Drill4Net.Injector.Core
{
    public class AssemblyReader
    {
        public AssemblyContext ReadAssembly(RunContext runCtx)
        {
            #region Paths
            var filePath = runCtx.SourceFile;
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not exists: [{filePath}]");

            //source
            var sourceDir = $"{Path.GetFullPath(Path.GetDirectoryName(filePath) ?? string.Empty)}\\";
            Environment.CurrentDirectory = sourceDir;
            var subjectName = Path.GetFileNameWithoutExtension(filePath);

            //destination
            var destDir = FileUtils.GetDestinationDirectory(runCtx.Options, sourceDir);
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            #endregion
            #region Version
            //need to know the version of the assembly before reading it via cecil
            var ext = Path.GetExtension(filePath);
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
            if (version == null || version.Target == AssemblyVersionType.NotIL ||
                (ext == ".exe" && version.Target == AssemblyVersionType.NetCore))
                return new AssemblyContext { Skipped = true };
            Console.WriteLine($"Version = {version}");
            #endregion
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
            var pdb = $"{subjectName}.pdb";
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
            }
            #endregion
            #endregion
            #region Reading
            // read subject assembly with symbols
            Log.Debug($"Reading file [{filePath}]");
            var assembly = AssemblyDefinition.ReadAssembly(filePath, readerParams);
            var asmCtx = new AssemblyContext(filePath, version, assembly)
            {
                SubjectName = subjectName,
                SourceDir = sourceDir,
                DestinationDir = destDir,
                IsNeedPdb = needPdb,
            };
            return asmCtx;
            #endregion
        }
    }
}
