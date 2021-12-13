using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.TestRunner.Core
{
    internal class CliController
    {
        private readonly Dictionary<RunInfo, List<string>> _runDatas;
        private readonly List<RunInfo> _infos;
        private readonly TestRunnerOptions _opts;
        private readonly Logger _logger;

        /******************************************************************/

        public CliController(List<RunInfo> infos, TestRunnerOptions opts)
        {
            _infos = infos ?? throw new ArgumentNullException(nameof(infos));
            _opts = opts ?? throw new ArgumentNullException(nameof(opts));
            _logger = new TypedLogger<CliController>(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER);
            _runDatas = new Dictionary<RunInfo, List<string>>();
        }

        /******************************************************************/

        internal void AddInfo(RunInfo info, List<string> argStrs)
        {
            if (_runDatas.ContainsKey(info))
                return;
            _runDatas.Add(info, argStrs);
        }

        internal void Start()
        {
            var globalParallelRestrict = _opts.DefaultParallelRestrict; //on Run level - for all specified directories
            var enumerator = _runDatas.GetEnumerator(); //it is safe due possible parallel using AddInfo method
            while (enumerator.MoveNext())
            {
                var runInfo = enumerator.Current.Key;
                var args = enumerator.Current.Value;
                var pids = RunTests(args); //the tests are run by VSTest CLI ("dotnet test <dll> <params> ...")
            }
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
