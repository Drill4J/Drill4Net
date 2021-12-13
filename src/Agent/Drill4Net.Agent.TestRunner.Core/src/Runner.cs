﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

/*** INFO
    automatic version tagger including Git info - https://github.com/devlooped/GitInfo
    semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
    the most common format is v0.0 (or just 0.0 is enough)
    to change semVer it is nesseccary to create appropriate tag and push it to remote repository
    patches'(commits) count starts with 0 again after new tag pushing
    For file version format exactly is digit
***/
[assembly: AssemblyFileVersion(CommonUtils.AssemblyFileGitVersion)]
[assembly: AssemblyInformationalVersion(CommonUtils.AssemblyGitVersion)]

namespace Drill4Net.Agent.TestRunner.Core
{
    //Swagger: http://localhost:8090/apidocs/index.html?url=./openapi.json

    /// <summary>
    /// Core runner for target's tests
    /// </summary>
    public class Runner
    {
        private readonly TestRunnerRepository _rep;
        private readonly Logger _logger;

        /***********************************************************************************/

        public Runner(TestRunnerRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<Runner>(_rep.Subsystem);
        }

        /***********************************************************************************/

        public async Task Run()
        {
            _logger.Debug("Wait for agents' initializing...");
            _rep.Start();

            _logger.Debug("Getting tests' run info...");
            var infos = await _rep.GetRunInfos().ConfigureAwait(false);
            if (infos.Count == 0)
            {
                _logger.Info("Nothing to run");
                return;
            }
            //
            _logger.Debug("Getting CLI run info...");
            try
            {
                var globalParallelRestrict = _rep.Options.DefaultParallelRestrict;
                foreach (var runInfo in infos)
                {
                    var args = GetRunArguments(runInfo);
                    var pids = RunTests(args); //the tests are run by VSTest CLI ("dotnet test <dll> <params> ...")
                }

                _logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Get CLI run info", ex);
            }
        }

        /// <summary>
        /// Get the arguments for running the test by VSTest CLI
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal List<string> GetRunArguments(RunInfo info)
        {
            var asmInfos = info.RunAssemblyInfos;
            var res = new List<string>();

            foreach (var asmInfo in asmInfos.Values)
            {
                var tests = asmInfo.Tests;
                var asmPath = FileUtils.GetFullPath(asmInfo.AssemblyPath, FileUtils.EntryDir);

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
                for (int i = 0; i < tests.Count; i++)
                {
                    string test = tests[i];

                    // test case -> just test name. Is it Guanito? No... SpecFlow's test cases contain bracket - so, VSTest break
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
