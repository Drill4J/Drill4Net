using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard.Debug
{
    class Program
    {
        private static string _targetPath;
        private static int _pointRange = 200;
        private static Dictionary<string, InjectedMethod> _methods;
        private static List<CrossPoint> _points;
        private const ConsoleColor INFO_COLOR = ConsoleColor.Cyan;
        private const ConsoleColor COLOR_DEFAULT = ConsoleColor.Green;

        /*********************************************************************************/

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            try
            {
                WriteMessage("Agent.Standard.Debug", ConsoleColor.Cyan);

                await Init();
                PrintMenu();
                Polling();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            WriteMessage("Done.", ConsoleColor.White);
        }

        private async static Task Init()
        {
            WriteMessage("Please wait for the init...", ConsoleColor.White);
            await Task.Delay(2500).ConfigureAwait(false); //wait for the reading

            StandardAgent.Init();

            //points' data (in fact, from the TestEngine's tree)
            var rep = StandardAgent.Repository;
            var tree = rep.ReadInjectedTree();
            const string moniker = "net5.0";
            var asmTree = tree.GetFrameworkRootDirectory(moniker);
            if (asmTree == null)
                throw new Exception($"Data for moniker {moniker} not found");
            _points = asmTree.GetAllPoints().ToList();
            var methList = asmTree.GetAllMethods().Where(a => !a.IsCompilerGenerated && a.Source.AccessType == AccessType.Public);
            _methods = new Dictionary<string, InjectedMethod>();
            foreach (var meth in methList)
            {
                var key = meth.Name;
                if (!_methods.ContainsKey(key))
                    _methods.Add(key, meth);
            }

            //TODO: from cfg!!!
            _targetPath = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Tests\TargetApps.Injected\Drill4Net.Target.Net50.App\net5.0\";

            ////debug
            //var treeCnv = new TreeConverter();
            //var types = tree.GetAllTypes().Where(a => a.Name == "CoverageTarget");
            //treeCnv.CreateCoverageRegistrator(new Abstract.Transfer.StartSessionPayload(), types);

            await Task.Delay(3500).ConfigureAwait(false); //wait for the admin side init

            //range
            WriteMessage($"\n  Framework: {moniker}", INFO_COLOR);
            WriteMessage($"  Assemblies: {asmTree.GetAllAssemblies().Count()}", INFO_COLOR);
            WriteMessage($"  Types: {asmTree.GetAllTypes().Count()}", INFO_COLOR);
            WriteMessage($"  Unique public methods: {_methods.Count}", INFO_COLOR);
            WriteMessage($"  Total cross-points: {_points.Count}", INFO_COLOR);
            //WriteMessage($"  Block size of cross-points: {_pointRange}", infoColor);
        }

        private static void Polling()
        {
            while (true)
            {
                WriteMessage("\nInput:");
                var input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (input == "q" || input == "Q")
                    break;
                //
                string output = null;
                try
                {
                    ProcessInput(input);
                }
                catch (Exception ex)
                {
                    output = $"error -> {ex.Message}";
                }
            }

            Console.ReadKey(true);
        }

        /// <summary>
        /// Sends to the Admin side the points.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static bool ProcessInput(string input)
        {
            return input switch
            {
                "?" or "help" => PrintMenu(),
                "print" or "list" => PrintMethods(),
                "save" => SaveTreeData(),
                _ => SendMethodData(input)
            };
        }

        private static bool SendMethodData(string callData)
        {
            //get info
            if (string.IsNullOrWhiteSpace(callData))
            {
                WriteMessage("No input", ConsoleColor.Red);
                return false;
            }
            var ar = callData.Split(' ');
            var name = ar[0].Trim();
            if (!_methods.ContainsKey(name))
            {
                WriteMessage("No such method", ConsoleColor.Red);
                return false;
            }
            var pars = new List<string>();
            if (ar.Length > 1)
            {
                foreach (var par in ar[1].Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(par))
                        pars.Add(par.Trim());
                }
            }

            //getting MethodInfo
            var method = _methods[name];
            MethodInfo methInfo;
            object target;
            try
            {
                WriteMessage($"Calling: {method.FullName}", ConsoleColor.White);
                var asmPath = Path.Combine(_targetPath, method.AssemblyName);
                var asm = Assembly.LoadFrom(asmPath);
                var typeName = method.BusinessType;
                var type = asm.GetType(typeName);
                methInfo = type.GetMethod(name);
                target = Activator.CreateInstance(asm.FullName, typeName).Unwrap();
            }
            catch (Exception ex)
            {
                WriteMessage($"Error of retrieving MethodInfo for [{method.FullName}] by reflection:\n{ex.Message}", ConsoleColor.Red);
                return false;
            }

            //calling
            try
            {
                var pars2 = ConvertToRealParameters(pars, method.Signature.Parameters);
                methInfo.Invoke(target, pars2 );
            }
            catch (Exception ex)
            {
                WriteMessage($"Error of the method [{method.FullName}] calling: [{callData}]:\n{ex.Message}", ConsoleColor.Red);
                return false;
            }
            return true;
        }

        #region Methods
        internal static bool PrintMethods()
        {
            WriteMessage("\n   ***  METHOD'S TREE  ***", ConsoleColor.Yellow);
            var methods = GetMethodInfos();
            var curAsm = "";
            var curType = "";
            var asmCounter = 1;
            var typeCounter = 1;
            var methCounter = 1;
            foreach (var meth in methods)
            {
                if (curAsm != meth.AssemblyName)
                {
                    curAsm = meth.AssemblyName;
                    WriteMessage($"\n-> ASSEMBLY_{asmCounter}: {curAsm}", ConsoleColor.White);
                    asmCounter++;
                }
                if (curType != meth.BusinessType)
                {
                    curType = meth.BusinessType;
                    WriteMessage($"\n   -- TYPE_{typeCounter}: {curType}", ConsoleColor.White);
                    typeCounter++;
                }
                WriteMessage($"                {methCounter}. {meth.Name} ({meth.Signature.Parameters})", ConsoleColor.White);
                methCounter++;
            }
            WriteMessage("\n   ***  END OF METHOD'S TREE  ***", ConsoleColor.Yellow);
            return true;
        }

        internal static bool SaveTreeData()
        {
            //data
            var methods = GetMethodInfos();
            var methCounter = 1;
            var data = new List<string>();
            var delim = ";";
            foreach (var meth in methods)
            {
                data.Add($"{methCounter}{delim}{meth.AssemblyName}{delim}{meth.BusinessType}{delim}{meth.Name}({meth.Signature.Parameters})");
                methCounter++;
            }

            //writing
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tree.csv");
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

        internal static List<InjectedMethod> GetMethodInfos()
        {
            return _methods.Values
                .OrderBy(a => a.AssemblyName)
                .ThenBy(a => a.BusinessType)
                .ThenBy(a => a.FullName)
                .ToList();
        }
        #endregion

        private static object[] ConvertToRealParameters(List<string> parVals, string parTypes)
        {
            if (string.IsNullOrWhiteSpace(parTypes))
                return null;
            object[] res = new object[parVals.Count];
            var types = parTypes.Split(',');
            for (var i = 0; i < parVals.Count; i++)
            {
                object obj = null;
                var val = parVals[i];
                if (val != "null")
                {
                    var type = types[i];
                    switch (type)
                    {
                        case "System.Boolean": obj = bool.Parse(val); break;
                        case "System.Byte": obj = byte.Parse(val); break;
                        case "System.UInt16": obj = ushort.Parse(val); break;
                        case "System.Int16": obj = short.Parse(val); break;
                        case "System.UInt32": obj = uint.Parse(val); break;
                        case "System.Int32": obj = int.Parse(val); break;
                        case "System.UInt64": obj = ulong.Parse(val); break;
                        case "System.Int64": obj = long.Parse(val); break;
                        case "System.Single": obj = float.Parse(val); break;
                        case "System.Double": obj = double.Parse(val); break;
                        case "System.Object":
                        case "System.String": obj = val; break;
                        default:
                            WriteMessage($"Unknown type: [{type}] for data [{val}]", ConsoleColor.DarkYellow);
                            break;
                    }
                }
                res[i] = obj;
            }
            return res;
        }

        private static bool PrintMenu()
        {
            const string mess = @"  *** First of all, start session on admin side...
  *** Enter 'print' or 'list' for the method listing
  *** Enter 'save' to save method's tree to the CSV file
  *** Enter order number of method from the listing with arguments for real probe's executing, e.g. 37 true
  *** Or input method name with arguments for such executing, e.g. IfElse_Consec_Full true,false
  *** Enter 'RunTests' to execute all methods of main target class (InjectTarget)
  *** Enter '?' or 'help' to print this menu
  *** Press q for exit";
            WriteMessage($"\n{mess}", ConsoleColor.Yellow);
            return true;
        }

        private static void WriteMessage(string mess, ConsoleColor color = COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = COLOR_DEFAULT;
        }

        ///// <summary>
        ///// Sends to the Admin side the portion of the points.
        ///// </summary>
        ///// <returns></returns>
        //private static bool SendPointBlock()
        //{
        //    if (_points.Count == 0)
        //    {
        //        WriteMessage("No more points!", ConsoleColor.Red);
        //        return false;
        //    }
        //    //
        //    var r = new Random(DateTime.Now.Millisecond);
        //    var end = Math.Min(_pointRange, _points.Count);
        //    for (var i = 0; i < end; i++)
        //    {
        //        var ind = r.Next(0, _points.Count);
        //        var point = _points[ind];
        //        _points.RemoveAt(ind);
        //        StandardAgent.RegisterStatic($"{point.PointUid}");
        //    }
        //    if (_points.Count == 0)
        //        WriteMessage("No more points!", ConsoleColor.Red);
        //    else
        //        WriteMessage($"Remaining points: {_points.Count}", ConsoleColor.Blue);
        //    return true;
        //}
    }
}