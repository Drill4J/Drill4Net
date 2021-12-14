using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Runner for tests with VSTest CLI
    /// </summary>
    internal class CliRunner
    {
        private readonly Logger _logger;

        /**********************************************************************/

        public CliRunner()
        {
            _logger = new TypedLogger<CliRunner>(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER);
        }

        /**********************************************************************/

        public void Start(List<DirectoryRunInfo> infos, bool runParallelRestrict, int degreeOfParallelism)
        {
            _logger.Debug("Starting...");

            var controller = new CliDispatcher();
            foreach (var runInfo in infos)
            {
                var args = GetRunArguments(runInfo);
                controller.AddInfo(runInfo, args);
            }
            controller.Start(runParallelRestrict, degreeOfParallelism);

            _logger.Debug("Finished");
        }

        /// <summary>
        /// Get the arguments for running the tests with VSTest CLI
        /// </summary>
        /// <param name="runInfo"></param>
        /// <returns>CLI strings to run tests, possibly for different assemblies or
        /// divided by chunks due too much long argument's length</returns>
        /// <exception cref="Exception"></exception>
        internal List<string> GetRunArguments(DirectoryRunInfo runInfo)
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
    }
}
