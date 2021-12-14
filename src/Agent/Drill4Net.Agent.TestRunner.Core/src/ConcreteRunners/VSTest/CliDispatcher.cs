using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Dispatcher for run and control the VSTest CLI consoles with tests
    /// </summary>
    internal class CliDispatcher
    {
        private readonly Dictionary<RunInfo, List<string>> _runDatas;
        private readonly Logger _logger;

        /******************************************************************/

        public CliDispatcher()
        {
            _logger = new TypedLogger<CliDispatcher>(CoreConstants.SUBSYSTEM_AGENT_TEST_RUNNER);
            _runDatas = new Dictionary<RunInfo, List<string>>();
        }

        /******************************************************************/

        internal void AddInfo(RunInfo info, List<string> argStrs)
        {
            if (_runDatas.ContainsKey(info))
                return;
            _runDatas.Add(info, argStrs);
        }

        internal void Start(bool runParallelRestrict)
        {
            var groups = CalculateGroups(_runDatas, runParallelRestrict);
            RunGroups(groups);
        }

        /// <summary>
        /// Calculate Run groups with argument lines and parallel execurion parameters
        /// </summary>
        /// <param name="runDatas"></param>
        /// <param name="runParallelRestrict">Parallel execurion restriction on Run level - for all specified directories</param>
        /// <returns></returns>
        internal List<List<(string Args, bool ParallelRestrict)>>
            CalculateGroups(Dictionary<RunInfo, List<string>> runDatas, bool runParallelRestrict)
        {
            var res = new List<List<(string, bool)>>();
            var enumerator = runDatas.GetEnumerator(); //it is safe due possible parallel using AddInfo method
            if (runParallelRestrict) //only sequential run on the whole Run level
            {

            }
            else //parallel on the whole Run level
            {
                var group = new List<(string, bool)>();
                res.Add(group);
                while (enumerator.MoveNext())
                {
                    var runInfo = enumerator.Current.Key;
                    var args = enumerator.Current.Value;
                    foreach (var arg in args)
                    {
                        var paralRestrict = runParallelRestrict || runInfo.DirectoryOptions.DefaultParallelRestrict;
                        group.Add((arg, paralRestrict));
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// It runs groups of CLI VSTest consoles and controls them with parallel options
        /// </summary>
        /// <param name="groups"></param>
        internal void RunGroups(List<List<(string Args, bool ParallelRestrict)>> groups)
        {
            //each separate group has parallel execution mode
            foreach (var group in groups)
            {
                var pids = new List<int>();
                foreach (var console in group)
                {
                    //one CLI console - one assembly with tests
                    var args = console.Args; //parallelization option for tests in assembly already is included in args
                    var pid = RunTests(args); //the tests are run by VSTest CLI ("dotnet test <dll> <params> ...")
                    if (pid == 0)
                        continue;
                    pids.Add(pid);
                }

                // we must wait here until all CLI are finish
                WaitForFinishingProcesses(pids);
            }
        }

        internal void WaitForFinishingProcesses(List<int> pids)
        {
            //TODO: loading new consoles after the gradual finishing of the current ones ???
            while (true)
            {
                for (int i = 0; i < pids.Count; i++)
                {
                    int pid = pids[i];
                    Process prc = null;
                    try
                    {
                        prc = Process.GetProcessById(pid);
                    }
                    catch { }
                    if (prc != null)
                        continue;
                    pids.RemoveAt(i);
                    i--;
                }
                if (pids.Count == 0)
                    return;
                Task.Delay(1000);
            }
        }

        /// <summary>
        /// Run the specified test in arguments by VSTest CLI
        /// </summary>
        /// <param name="args">Run string with argument for VSTest CLI</param>
        internal int RunTests(string args)
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
                var pid = process.Id;
                _logger.Info($"Process {pid} started for [{args}]");
                return pid;
            }
            else
            {
                _logger.Error($"Process does not started for [{args}]");
                return 0;
            }
        }
    }
}
