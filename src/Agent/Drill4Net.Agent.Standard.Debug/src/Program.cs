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
                WriteMessage($"\nUnique public methods: {_methods.Count}", ConsoleColor.Cyan);
                WriteMessage($"Total cross-points: {_points.Count}", ConsoleColor.Cyan);
                //WriteMessage($"Input point count for the extraction range [{_pointRange}]: ", ConsoleColor.Yellow);
                //var expr = Console.ReadLine()?.Trim();
                //if (int.TryParse(expr, out var pntCnt))
                //{
                //    if (pntCnt > 0)
                //        _pointRange = pntCnt;
                //}
                WriteMessage($"Block size of cross-points: {_pointRange}", ConsoleColor.Cyan);

                // info
                const string mess = @"  *** First of all, start session on admin side...
  *** Press 1 to send some portion of random target method's cross-points
  *** Or input method name with arguments for real probe's executing, e.g. IfElse_Consec_Full true,false
  *** Press q for exit";
                WriteMessage($"\n{mess}", ConsoleColor.Yellow);

                //polling
                while (true)
                {
                    WriteMessage("\nInput:");
                    var expr = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(expr))
                        continue;
                    if (expr == "q" || expr == "Q")
                        break;
                    //
                    string output = null;
                    try
                    {
                        SendData(expr);
                    }
                    catch (Exception ex)
                    {
                        output = $"error -> {ex.Message}";
                    }
                }

                Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            WriteMessage("Done.", ConsoleColor.White);
        }

        /// <summary>
        /// Sends to the Admin side the points.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static bool SendData(string input)
        {
            if (input == "1")
                return SendPointBlock();
            else
                return SendMethodData(input);
        }

        /// <summary>
        /// Sends to the Admin side the portion of the points.
        /// </summary>
        /// <returns></returns>
        private static bool SendPointBlock()
        {
            if (_points.Count == 0)
            {
                WriteMessage("No more points!", ConsoleColor.Red);
                return false;
            }
            //
            var r = new Random(DateTime.Now.Millisecond);
            var end = Math.Min(_pointRange, _points.Count);
            for (var i = 0; i < end; i++)
            {
                var ind = r.Next(0, _points.Count);
                var point = _points[ind];
                _points.RemoveAt(ind);
                StandardAgent.RegisterStatic($"{point.PointUid}");
            }
            if (_points.Count == 0)
                WriteMessage("No more points!", ConsoleColor.Red);
            else
                WriteMessage($"Remaining points: {_points.Count}", ConsoleColor.Blue);
            return true;
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
            var name = ar[0];
            if (!_methods.ContainsKey(name))
            {
                WriteMessage("No such method", ConsoleColor.Red);
                return false;
            }
            var pars = new List<string>();
            if (ar.Length > 1)
            {
                foreach (var par in ar[1].Split(','))
                    if(!string.IsNullOrWhiteSpace(par))
                        pars.Add(par.Trim());
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

        private static void WriteMessage(string mess, ConsoleColor color = COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = COLOR_DEFAULT;
        }
    }
}