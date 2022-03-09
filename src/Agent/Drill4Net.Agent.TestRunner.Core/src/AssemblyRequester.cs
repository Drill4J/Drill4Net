using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.TestRunner.Core
{
    internal class AssemblyRequester
    {
        private readonly Logger _logger;

        /******************************************************************/

        public AssemblyRequester()
        {
            _logger = new TypedLogger<CliDispatcher>(CoreConstants.SUBSYSTEM_TEST_RUNNER);
        }

        /******************************************************************/

        internal async Task<List<string>> GetAssemblyTests(string asmPath)
        {
            if(string.IsNullOrWhiteSpace(asmPath))
                throw new ArgumentNullException("Path of test assembly is empty");
            if (!File.Exists(asmPath))
                throw new ArgumentNullException("Path of test assembly is empty");
            //
            var res = new List<string>();
            var args = $"/C dotnet test \"{asmPath}\" --list-tests";
            var prc = RunVsTest(args);
            if(prc == null)
                return res;
            string standard_output;
            bool start = false;
            while ((standard_output = prc.StandardOutput.ReadLine()) != null)
            {
                if (start)
                    res.Add(standard_output?.Trim());
                if (standard_output.EndsWith("are available:"))
                    start = true;
                if (standard_output.Contains("xx"))
                {
                    //do something
                    break;
                }
            }
            return res;
        }

        /// <summary>
        /// Run the VSTest CLI for retrieving the tests' list
        /// </summary>
        /// <param name="args">Run string with argument for VSTest CLI</param>
        internal Process RunVsTest(string args)
        {
            _logger.Debug($"Running VSTest with args: [{args}]");

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = args,
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    //RedirectStandardError = true
                }
            };

            if (process.Start())
            {
                var pid = process.Id;
                _logger.Debug($"Process {pid} started for [{args}]");
                return process;
            }
            else
            {
                _logger.Error($"Process does not started for [{args}]");
                return null;
            }
        }
    }
}
