﻿using System.IO;
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
        /// <returns>New path for the saved assembly</returns>
        public string SaveAssembly(RunContext runCtx, AssemblyContext asmCtx, string modifiedPath = null)
        {
            if (runCtx.AssemblyPaths.ContainsKey(asmCtx.DestinationKey))
                return null;
            //
            string origFilePath;
            string destDir;
            if (string.IsNullOrWhiteSpace(modifiedPath))
            {
                origFilePath = asmCtx.SourceFile;
                destDir = asmCtx.DestinationDir;
                modifiedPath = GetDestFileName(origFilePath, destDir);
            }
            else
            {
                origFilePath = modifiedPath;
                destDir = Path.GetDirectoryName(modifiedPath);
            }
            //
            var writeParams = new WriterParameters();
            if (asmCtx.IsNeedPdb)
            {
                var subjectName = Path.GetFileNameWithoutExtension(origFilePath);
                var pdbPath = Path.Combine(destDir, subjectName + ".pdb");
                #pragma warning disable DF0033 // Marks undisposed objects assinged to a property, originated from a method invocation.
                writeParams.SymbolStream = File.Create(pdbPath);
                #pragma warning restore DF0033 // Marks undisposed objects assinged to a property, originated from a method invocation.
                writeParams.WriteSymbols = true;
                // net core uses portable pdb
                writeParams.SymbolWriterProvider = new PortablePdbWriterProvider(); //TODO: check it for NetFx (seems it need another type o provider)!
            }

            try
            {
                asmCtx.Definition.Write(modifiedPath, writeParams);
            }
            catch//(Exception ex)
            {
                //because after the error the content of the current file could have been erased - it will be old now
                File.Copy(origFilePath, modifiedPath, true);
                throw;
            }

            runCtx.AssemblyPaths.Add(asmCtx.DestinationKey, modifiedPath);
            return modifiedPath;
        }

        public string GetDestFileName(string origFilePath, string destDir)
        {
            var ext = Path.GetExtension(origFilePath);
            var subjectName = Path.GetFileNameWithoutExtension(origFilePath);
            return $"{Path.Combine(destDir, subjectName)}{ext}";
        }
    }
}
