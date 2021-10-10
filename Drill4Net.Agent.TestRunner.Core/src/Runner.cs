﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.TestRunner.Core
{
    //Swagger: http://localhost:8090/apidocs/index.html?url=./openapi.json

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
            _logger.Debug("Getting the build summaries...");

            try
            {
                var (runType, tests) = await _rep.GetRunToTests();
                if (runType == RunningType.Nothing)
                    return;
                var args = GetRunArguments(tests);
                StartTests(args); //we need test's names here for its runs by CLI ("dotnet test ...")

                _logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Get builds' summary", ex);
            }
        }

        internal string GetRunArguments(IList<string> tests)
        {
            // prefix "/C" - is for running in the CMD
            var args = $"/C dotnet test \"{_rep.Options.FilePath}\"";
            if (tests?.Any() != true)
                return args;
            args += " --filter \"";
            for (int i = 0; i < tests.Count; i++)
            {
                string test = tests[i];
                //
                var ind = test.IndexOf("(");
                if(ind != -1)
                    test = test.Substring(0, ind);
                //
                args += $"DisplayName~{test}";
                if (i < tests.Count - 1)
                    args += "|";
                else
                    args += "\"";
            }
            if (args.Length > 32767)
                throw new Exception("Argument's length exceeds the maximum. We need improve algorythm (to do some separate runnings)");
            return args;
        }

        internal void StartTests(string args)
        {
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
            process.Start();
        }
    }
}
