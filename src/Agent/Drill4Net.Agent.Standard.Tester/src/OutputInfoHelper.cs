using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Agent.Standard.Tester
{
    internal static class OutputInfoHelper
    {
        internal static bool PrintTreeInfo(TreeInfo treeInfo)
        {
            WriteMessage($"\n  Tree data:", ConsoleColor.Yellow);
            WriteMessage($"  Name: treeInfo.injSolution.Name}", COLOR_INFO);
            if (!string.IsNullOrWhiteSpace(treeInfo.injSolution.Description))
                WriteMessage($"  Description: treeInfo.injSolution.Description}", COLOR_INFO);
            WriteMessage($"  Orig destination: {treeInfo.injSolution.DestinationPath}", COLOR_INFO);

            //TODO: fix empty FinishTime
            WriteMessage($"  Processed time: {treeInfo.injSolution.FinishTime ?? treeInfo.injSolution.StartTime}", COLOR_INFO);

            var dirs = treeInfo.injSolution.GetDirectories(); //dirs on the first level
            if (dirs.Any())
            {
                WriteMessage($"\n  Inner directories (may be several versions of framework): {dirs.Count()}", ConsoleColor.Yellow);
                foreach (var dir in dirs)
                    WriteMessage($"  {dir.Name}", COLOR_INFO);
            }

            WriteMessage($"\n  Testing: ", ConsoleColor.Yellow);
            WriteMessage($"  Folder : {treeInfo.opts.TreeFolder}", COLOR_INFO);
            WriteMessage($"  Assemblies: {treeInfo.injDirectory.GetAllAssemblies().Count()}", COLOR_INFO);
            WriteMessage($"  Types: {treeInfo.injDirectory.GetAllTypes().Count()}", COLOR_INFO);
            WriteMessage($"  Unique public methods: {treeInfo.methods.Count}", COLOR_INFO);
            WriteMessage($"  Total cross-points: treeInfo.points.Count}", COLOR_INFO);
            //WriteMessage($"  Block size of cross-points: treeInfo.pointRange}", infoColor);
            return true;
        }

        internal static bool PrintMenu()
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
            WriteMessage($"\n{mess}", ConsoleColor.Yellow);
            return true;
        }
        internal static bool PrintTree(List<InjectedMethod> methodSorted)
        {
            WriteMessage("\n   ***  METHOD'S TREE  ***", ConsoleColor.Yellow);
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
                    WriteMessage($"\n--> ASSEMBLY_{asmCounter}: {curAsm}", ConsoleColor.White);
                }
                if (curType != meth.BusinessType)
                {
                    curType = meth.BusinessType;
                    typeCounter++;
                    WriteMessage($"\n   -- TYPE_{asmCounter}.{typeCounter}: {curType}", ConsoleColor.White);
                }
                methCounter++;
                WriteMessage($"        {methCounter}. {meth.Name} ({meth.Signature.Parameters})", ConsoleColor.White);
            }
            WriteMessage("\n   ***  END OF METHOD'S TREE  ***", ConsoleColor.Yellow);
            return true;
        }
        internal static bool SaveTreeData(List<InjectedMethod> methodSorted, TesterOptions opts)
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
            var path = Path.Combine(opts.CSV ?? FileUtils.ExecutingDir, "tree.csv"); //TODO: to const
            try
            {
                File.WriteAllLines(path, data);
                WriteMessage($"CSV was written to: {path}", ConsoleColor.Yellow);
            }
            catch (Exception ex)
            {
                WriteMessage($"Error: {ex.Message}", ConsoleColor.Red);
            }
            return true;
        }

        internal static void WriteMessage(string mess, ConsoleColor color = COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = COLOR_DEFAULT;
        }
    }
}
