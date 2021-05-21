using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Drill4Net.Injector.Core
{
    public class AssemblyWriter
    {
        /// <summary>
        /// Save injected assembly by new path
        /// </summary>
        /// <param name="runCtx"></param>
        /// <param name="asmCtx"></param>
        /// <returns></returns>
        public string SaveAssembly(RunContext runCtx, AssemblyContext asmCtx)
        {
            var origFilePath = asmCtx.SourceFile;
            var destDir = asmCtx.DestinationDir;
            var modifiedPath = GetDestFileName(origFilePath, destDir);
            var writeParams = new WriterParameters();
            if (asmCtx.IsNeedPdb)
            {
                var subjectName = Path.GetFileNameWithoutExtension(origFilePath);
                var pdbPath = Path.Combine(destDir, subjectName + ".pdb");
                writeParams.SymbolStream = File.Create(pdbPath);
                writeParams.WriteSymbols = true;
                // net core uses portable pdb
                writeParams.SymbolWriterProvider = new PortablePdbWriterProvider();
            }
            var asm = asmCtx.Definition;
            asm.Write(modifiedPath, writeParams);
            runCtx.Paths.Add(asm.FullName, modifiedPath);
            return modifiedPath;
        }

        public string GetDestFileName(string origFilePath, string destDir)
        {
            var ext = Path.GetExtension(origFilePath);
            var subjectName = Path.GetFileNameWithoutExtension(origFilePath);
            var modifiedPath = $"{Path.Combine(destDir, subjectName)}{ext}";
            return modifiedPath;
        }
    }
}
