using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyInformationalVersion(
      ThisAssembly.Git.SemVer.Major + "." +
      ThisAssembly.Git.SemVer.Minor + "." +
      ThisAssembly.Git.SemVer.Patch + "-" +
      ThisAssembly.Git.Branch + "+" +
      ThisAssembly.Git.Commit)]

namespace Drill4Net.Agent.Standard.Debug
{
    class Program
    {
        private static InjectedSolution _injSolution;
        private static InjectedDirectory _injDirectory;
        private static string _targetPath;
        private static int _pointRange = 200;
        private static Dictionary<string, InjectedMethod> _methods;
        private static List<InjectedMethod> _methodSorted;
        private static Dictionary<int, InjectedMethod> _methodByOrderNumber;
        private static List<CrossPoint> _points;
        private static TesterOptions _opts;
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
                SetTile();
                await Init();
                PrintTreeInfo();
                PrintMenu();
                Polling();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            WriteMessage("Done.", ConsoleColor.White);
        }

        private static void SetTile()
        {
            var version = GetAppVersion();
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var title = $"{appName} {version}";
            Console.Title = title;
            WriteMessage(title, ConsoleColor.Cyan);
        }

        private async static Task Init()
        {
            WriteMessage("Please wait for the init...", ConsoleColor.White);
            await Task.Delay(3000).ConfigureAwait(false); //wait for the reading

            StandardAgent.Init();

            _opts = GetOptions();
            _targetPath = _opts.CurrentDirectory;

            //points' data (in fact, from the TestEngine's tree)
            var rep = StandardAgent.Repository;
            _injSolution = rep.ReadInjectedTree();
            _injDirectory = _injSolution.GetDirectories().FirstOrDefault(a => a.Name == _opts.TreeFolder);
            if (_injDirectory == null)
                throw new Exception($"Directory in the Tree data not found: [{_targetPath}]");
            _points = _injDirectory.GetAllPoints().ToList();

            var methList = _injDirectory.GetAllMethods().Where(a => !a.IsCompilerGenerated && a.Source.AccessType == AccessType.Public);
            _methods = new Dictionary<string, InjectedMethod>();
            foreach (var meth in methList)
            {
                var key = meth.Name;
                if (!_methods.ContainsKey(key))
                    _methods.Add(key, meth);
            }
            _methodSorted = GetSortedMethods();
            _methodByOrderNumber = GetMethodByOrderNumber(_methodSorted);

            await Task.Delay(3500).ConfigureAwait(false); //wait for the admin side init
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
                _ => CallMethod(input)
            };
        }

        private static TesterOptions GetOptions()
        {
            var cfgPath = Path.Combine(FileUtils.GetExecutionDir(), "app.yml");
            var deser = new YamlDotNet.Serialization.Deserializer();
            return deser.Deserialize<TesterOptions>(File.ReadAllText(cfgPath));
        }

        private static bool CallMethod(string callData)
        {
            //get info
            if (string.IsNullOrWhiteSpace(callData))
            {
                WriteMessage("No input", ConsoleColor.Red);
                return false;
            }
            var ar = callData.Split(' ');
            var name = ar[0].Trim();

            //by order number?
            if (int.TryParse(name, out int number))
            {
                if (!_methodByOrderNumber.ContainsKey(number))
                {
                    WriteMessage($"No such order number: {number}", ConsoleColor.Red);
                    return false;
                }
                name = _methodByOrderNumber[number].Name;
            }

            //by name
            if (!_methods.ContainsKey(name))
            {
                WriteMessage("No such method", ConsoleColor.Red);
                return false;
            }

            //parameters
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
            var curAsm = "";
            var curType = "";
            var asmCounter = 1;
            var typeCounter = 1;
            var methCounter = 1;
            foreach (var meth in _methodSorted)
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
            var methCounter = 1;
            var data = new List<string>();
            const string delim = ";";
            foreach (var meth in _methodSorted)
            {
                data.Add($"{methCounter}{delim}{meth.AssemblyName}{delim}{meth.BusinessType}{delim}{meth.Name}({meth.Signature.Parameters})");
                methCounter++;
            }

            //writing
            var path = Path.Combine(_opts.CSV ?? FileUtils.GetExecutionDir(), "tree.csv");
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

        internal static List<InjectedMethod> GetSortedMethods()
        {
            return _methods.Values
                .OrderBy(a => a.AssemblyName)
                .ThenBy(a => a.BusinessType)
                .ThenBy(a => a.FullName)
                .ToList();
        }

        internal static Dictionary<int, InjectedMethod> GetMethodByOrderNumber(List<InjectedMethod> sorted)
        {
            var res = new Dictionary<int, InjectedMethod>();
            for (var i = 0; i < sorted.Count; i++)
                res.Add(i + 1, sorted[i]);
            return res;
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

        private static bool PrintTreeInfo()
        {
            WriteMessage($"\n  Tree data:", ConsoleColor.Yellow);
            WriteMessage($"  Name: {_injSolution.Name}", INFO_COLOR);
            if(!string.IsNullOrWhiteSpace(_injSolution.Description))
                WriteMessage($"  Description: {_injSolution.Description}", INFO_COLOR);
            WriteMessage($"  Orig destination: {_injSolution.DestinationPath}", INFO_COLOR);

            //TODO: fix empty FinishTime
            WriteMessage($"  Processed time: {_injSolution.FinishTime ?? _injSolution.StartTime}", INFO_COLOR);

            var dirs = _injSolution.GetDirectories(); //dirs on the first level
            WriteMessage($"  Top folders: {dirs.Count()}", INFO_COLOR);

            WriteMessage($"\n  Current folder : {_opts.TreeFolder}", INFO_COLOR);
            WriteMessage($"  Assemblies: {_injDirectory.GetAllAssemblies().Count()}", INFO_COLOR);
            WriteMessage($"  Types: {_injDirectory.GetAllTypes().Count()}", INFO_COLOR);
            WriteMessage($"  Unique public methods: {_methods.Count}", INFO_COLOR);
            WriteMessage($"  Total cross-points: {_points.Count}", INFO_COLOR);
            //WriteMessage($"  Block size of cross-points: {_pointRange}", infoColor);

            if (dirs.Any())
            {
                WriteMessage($"\n  Inner directories (may be several versions of framework): ", ConsoleColor.Yellow);
                foreach(var dir in dirs)
                    WriteMessage($"  {dir.Name}", INFO_COLOR);
            }
            return true;
        }

        private static bool PrintMenu()
        {
            const string mess = @"  *** First of all, start session on admin side...
  *** Enter 'info' for the tree info
  *** Enter 'print' or 'list' for the methods listing
  *** Enter 'save' to save method's tree to the CSV file
  *** Enter order number of method from the listing with arguments for real probe's executing, e.g. 37 true
  *** Or input method name with arguments for such executing, e.g. IfElse_Consec_Full true,false
  *** Enter 'RunTests' to execute all methods of main target class (InjectTarget)
  >>> Please, call yet the methods only for the InjectTarget type! 
  *** Enter '?' or 'help' to print this menu
  *** Press q for exit";
            WriteMessage($"\n{mess}", ConsoleColor.Yellow);
            return true;
        }

        internal static string GetAppVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            return FileUtils.GetProductVersion(asm.Location);
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