using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.TestRunner.Core
{
    internal class RunCliController
    {
        private readonly List<RunInfo> _infos;
        private readonly TestRunnerOptions _opts;
        private readonly Logger _logger;

        /**********************************************************************/

        public RunCliController(List<RunInfo> infos, TestRunnerOptions opts)
        {
            _opts = opts ?? throw new ArgumentNullException(nameof(opts));
            _infos = infos ?? throw new ArgumentNullException(nameof(infos));
            _logger = new TypedLogger<RunCliController>(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER);
        }

        /**********************************************************************/

        public void Start()
        {
            _logger.Debug("Starting...");
            var globalParallelRestrict = _opts.DefaultParallelRestrict; //on Run level - for all specified directories
            foreach (var runInfo in _infos)
            {
                var args = GetRunArguments(runInfo);
                var pids = RunTests(args); //the tests are run by VSTest CLI ("dotnet test <dll> <params> ...")
            }
            _logger.Debug("Finished");
        }

        /// <summary>
        /// Get the arguments for running the tests with VSTest CLI
        /// </summary>
        /// <param name="runInfo"></param>
        /// <returns>CLI strings to run tests, possibly for different assemblies or
        /// divided by chunks due too much long argument's length</returns>
        /// <exception cref="Exception"></exception>
        internal List<string> GetRunArguments(RunInfo runInfo)
        {
            var asmInfos = runInfo.RunAssemblyInfos;
            var res = new List<string>();

            foreach (var asmInfo in asmInfos.Values)
            {
                var tests = asmInfo.Tests;
                var asmPath = FileUtils.GetFullPath(Path.Combine(runInfo.DirectoryOptions.Path, asmInfo.AssemblyName), FileUtils.EntryDir);

                // prefix "/C" - is for running in the CMD
                var args = $"/C dotnet test \"{asmPath}\"";
                if (tests?.Any() != true)
                {
                    args = AddPostfixArgs(args, asmInfo);
                    res.Add(args);
                    continue;
                }
                //
                args += " --filter \"";
                _logger.Info($"Assembly: {asmPath} -> {tests.Count} tests");
                //TODO: dividing the too much long string to several argument strings
                for (int i = 0; i < tests.Count; i++)
                {
                    string test = tests[i];

                    // test case -> just test name. Is it Guanito? No... SpecFlow's test cases contain bracket - so, VSTest breaks
                    var ind = test.IndexOf("("); //after ( the parameters of case followed 
                    if (ind != -1)
                        test = test[..ind];
                    if (test.EndsWith(":")) //it can be so...
                        test = test[0..^1];
                    //
                    test = test.Replace(",", "%2C").Replace("\"", "\\\"").Replace("!", "\\!"); //need escaping
                    //FullyQualifiedName is full type name - for exactly comparing, as =, we need name with namespace
                    //TODO: = comparing with real namespaces
                    args += $"FullyQualifiedName~.{test}";
                    if (i < tests.Count - 1)
                        args += "|";
                    else
                        args += "\"";
                }

                args = AddPostfixArgs(args, asmInfo);
                res.Add(args);
            }
            return res;
        }

        internal string AddPostfixArgs(string args, RunAssemblyInfo asmInfo)
        {
            args += $" {GetLoggerParameters()} {GetParallelRunParameters(asmInfo.MustSequential)}"; //RunConfiguration must be at the end of args
            if (args.Length > 32767) //TODO: split tests and create separate cmd processes
                throw new Exception("Argument's length exceeds the maximum. We need improve algorithm (to do some separate runnings)");
            return args;
        }

        internal string GetParallelRunParameters(bool mustSequential)
        {
            //TODO: depending on the type of test framework: xUnit still disable, others aren't
            return $"-- RunConfiguration.DisableParallelization={mustSequential}";
        }

        internal string GetLoggerParameters()
        {
            return "--logger \"console;verbosity=detailed\"";
        }

        /// <summary>
        /// Run the specified test in arguments by VSTest CLI
        /// </summary>
        /// <param name="argsList"></param>
        internal List<int> RunTests(List<string> argsList)
        {
            var pids = new List<int>();

            //TODO: restrict count of simultaneously running cmd processes
            foreach (var args in argsList)
            {
                _logger.Debug($"Running tests with args: [{args}]");

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = "cmd.exe",
                        Arguments = args,
                        CreateNoWindow = false,
                        UseShellExecute = true,
                    }
                };

                if (process.Start())
                {
                    pids.Add(process.Id);
                    _logger.Info($"Process started for [{args}]");
                }
                else
                {
                    _logger.Error($"Process does not started for [{args}]");
                }
            }
            return pids;
        }
    }
}
