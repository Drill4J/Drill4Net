using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard.Tester
{
    /// <summary>
    /// Functions for outputting data
    /// </summary>
    internal class OutputInfoHelper
    {
        internal bool PrintTreeInfo(TesterTreeInfo treeInfo)
        {
            WriteMessage($"\n  Tree data:", TesterConstants.COLOR_TEXT_HIGHLITED);
            WriteMessage($"  Name: {treeInfo.InjSolution.Name}", TesterConstants.COLOR_INFO);
            if (!string.IsNullOrWhiteSpace(treeInfo.InjSolution.Description))
                WriteMessage($"  Description: {treeInfo.InjSolution.Description}", TesterConstants.COLOR_INFO);
            WriteMessage($"  Orig destination: {treeInfo.InjSolution.DestinationPath}", TesterConstants.COLOR_INFO);

            //TODO: fix empty FinishTime
            WriteMessage($"  Processed time: {treeInfo.InjSolution.FinishTime ?? treeInfo.InjSolution.StartTime}", TesterConstants.COLOR_INFO);

            var dirs = treeInfo.InjSolution.GetDirectories(); //dirs on the first level
            if (dirs.Any())
            {
                WriteMessage($"\n  Inner directories (may be several versions of framework): {dirs.Count()}", TesterConstants.COLOR_TEXT_HIGHLITED);
                foreach (var dir in dirs)
                    WriteMessage($"  {dir.Name}", TesterConstants.COLOR_INFO);
            }

            WriteMessage($"\n  Testing: ", TesterConstants.COLOR_TEXT_HIGHLITED);
            WriteMessage($"  Folder : {treeInfo.Opts.Moniker}", TesterConstants.COLOR_INFO);
            WriteMessage($"  Assemblies: {treeInfo.InjDirectory.GetAllAssemblies().Count()}", TesterConstants.COLOR_INFO);
            WriteMessage($"  Types: {treeInfo.InjDirectory.GetAllTypes().Count()}", TesterConstants.COLOR_INFO);
            WriteMessage($"  Unique public methods: {treeInfo.Methods.Count}", TesterConstants.COLOR_INFO);
            WriteMessage($"  Total cross-points: {treeInfo.Points.Count}", TesterConstants.COLOR_INFO);
            //WriteMessage($"  Block size of cross-points: treeInfo.pointRange}", infoColor);
            return true;
        }

        internal bool PrintMenu()
        {
            const string mess = @"  *** First of all, start session on admin side...
  *** Enter 'info' for the tree info.
  *** Enter 'tree' or 'list' for the methods listing.
  *** Enter 'save' to save method's tree to the CSV file.
  *** Enter order number of method from the listing with arguments for real probe's executing, e.g. 37 true
  *** Or input method name with arguments for such executing, e.g. IfElse_Consec_Full true,false
      You can even enter it by C# syntax copied from the source code: IfElse_Consec_Full(true,false)
      with or withot ; after this expression and even with leading await keyword.
  *** Enter 'RunTests' to execute all methods of main target class (InjectTarget).
  >>> Please, call the methods only for the InjectTarget type yet! 
  *** Enter '?' or 'help' to print this menu.
  *** Press q for exit.";
            WriteMessage($"\n{mess}", TesterConstants.COLOR_TEXT_HIGHLITED);
            return true;
        }

        internal bool PrintTree(List<InjectedMethod> methodSorted)
        {
            WriteMessage("\n   ***  METHOD'S TREE  ***", TesterConstants.COLOR_TEXT_HIGHLITED);
            var curAsm = "";
            var curType = "";
            var asmCounter = 0;
            var typeCounter = 0;
            var methCounter = 0;
            foreach (var meth in methodSorted)
            {
                if (curAsm != meth.AssemblyName)
                {
                    curAsm = meth.AssemblyName;
                    asmCounter++;
                    typeCounter = 0;
                    WriteMessage($"\n--> ASSEMBLY_{asmCounter}: {curAsm}", TesterConstants.COLOR_TEXT);
                }
                if (curType != meth.BusinessType)
                {
                    curType = meth.BusinessType;
                    typeCounter++;
                    WriteMessage($"\n   -- TYPE_{asmCounter}.{typeCounter}: {curType}", TesterConstants.COLOR_TEXT);
                }
                methCounter++;
                WriteMessage($"        {methCounter}. {meth.Name} ({meth.Signature.Parameters})", TesterConstants.COLOR_TEXT);
            }
            WriteMessage("\n   ***  END OF METHOD'S TREE  ***", TesterConstants.COLOR_TEXT_HIGHLITED);
            return true;
        }

        internal bool SaveTreeData(List<InjectedMethod> methodSorted, TesterOptions opts)
        {
            //data
            var methCounter = 1;
            var data = new List<string>();
            const string delim = ";";
            foreach (var meth in methodSorted)
            {
                data.Add($"{methCounter}{delim}{meth.AssemblyName}{delim}{meth.BusinessType}{delim}{meth.Name}({meth.Signature.Parameters})");
                methCounter++;
            }

            //writing
            var path = Path.Combine(opts.CSV ?? FileUtils.ExecutingDir, TesterConstants.CSV_NAME);
            try
            {
                File.WriteAllLines(path, data);
                WriteMessage($"CSV was written to: {path}", TesterConstants.COLOR_TEXT_HIGHLITED);
            }
            catch (Exception ex)
            {
                WriteMessage($"Error: {ex.Message}", TesterConstants.COLOR_ERROR);
            }
            return true;
        }

        internal void WriteMessage(string mess, ConsoleColor color = TesterConstants.COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = TesterConstants.COLOR_DEFAULT;
        }
    }
}
