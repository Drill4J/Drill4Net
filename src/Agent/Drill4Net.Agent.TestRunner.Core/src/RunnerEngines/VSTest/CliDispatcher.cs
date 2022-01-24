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
        private readonly Dictionary<DirectoryRunInfo, List<string>> _dirRunDatas;
        private readonly Logger _logger;

        /******************************************************************/

        public CliDispatcher()
        {
            _logger = new TypedLogger<CliDispatcher>(CoreConstants.SUBSYSTEM_TEST_RUNNER);
            _dirRunDatas = new Dictionary<DirectoryRunInfo, List<string>>();
        }

        /******************************************************************/

        internal void AddInfo(DirectoryRunInfo dirRunInfo, List<string> argStrs)
        {
            if (_dirRunDatas.ContainsKey(dirRunInfo))
                return;
            _dirRunDatas.Add(dirRunInfo, argStrs);
        }

        /// <summary>
        /// Caclulate the VSTest CLI's groups, start and control them
        /// </summary>
        /// <param name="runParallelRestrict">Parallel execurion restriction on Run level - for all specified directories</param>
        /// <param name="degreeOfParallelism">Degree of parallelism for "places" where it possibly</param>
        internal void Start(bool runParallelRestrict, int degreeOfParallelism)
        {
            var groups = CalculateGroups(_dirRunDatas, runParallelRestrict);
            RunGroups(groups, degreeOfParallelism);
        }

        /// <summary>
        /// Calculate Run groups with argument lines and parallel execurion parameters
        /// </summary>
        /// <param name="dirRunDatas"></param>
        /// <param name="runParallelRestrict">Parallel execurion restriction on Run level - for all specified directories</param>
        /// <param name="degreeOfParallelism">Degree of parallelism for "places" where it possibly</param>
        /// <returns></returns>
        internal List<List<string>> CalculateGroups(Dictionary<DirectoryRunInfo, List<string>> dirRunDatas,
            bool runParallelRestrict)
        {
            Dictionary<string, List<string>> groups = new();
            List<string> group = null; //CLI consoles with some parallelization option
            if (!runParallelRestrict) //all directories can run tests in parallel THEMSELVES
            {
                group = new();
                groups.Add("default", group);
            }
            //
            var enumerator = dirRunDatas.GetEnumerator(); //need safety due possible parallel using AddInfo method
            while (enumerator.MoveNext())
            {
                var dirRunInfo = enumerator.Current.Key;
                var argList = enumerator.Current.Value;

                //parallel mode - current directory (for assemblies' run)
                var dirParalRestrictOption = dirRunInfo.DirectoryOptions.DefaultParallelRestrict;
                var dirParalRestrictActual = dirParalRestrictOption == true || (dirParalRestrictOption == null && runParallelRestrict);

                if (runParallelRestrict || dirParalRestrictActual) //level: directories OR assemblies
                {
                    var key = dirRunInfo.DirectoryOptions.Directory;
                    if (dirParalRestrictActual)
                        key += "/" + dirRunInfo.AssemblyOptions.DefaultAssemblyName;

                    if (groups.ContainsKey(key))
                    {
                        group = groups[key];
                    }
                    else
                    {
                        group = new();
                        groups.Add(key, group);
                    }
                }

                foreach (var arg in argList)
                    group.Add(arg);
            }
            return groups.Values.ToList();
        }

        /// <summary>
        /// It runs groups of CLI VSTest consoles and controls them with parallel options
        /// </summary>
        /// <param name="groups">Groups of CLI calls</param>
        /// <param name="degreeOfParallelism">Degree of parallelism for "places" where it possibly</param>
        internal void RunGroups(List<List<string>> groups, int degreeOfParallelism)
        {
            if (degreeOfParallelism < 1)
                degreeOfParallelism = 1;

            //groups run consoles sequentially among themselves
            //each separate group run consoles simultaneously
            for (int grpInd = 0; grpInd < groups.Count; grpInd++)
            {
                var group = groups[grpInd];
                var chunks = Extensions.Chunk(group, degreeOfParallelism).ToList();
                for(var chInd = 0; chInd < chunks.Count; chInd++)
                {
                    var chunk = chunks[chInd];
                    var pids = new List<int>();
                    foreach (var args in chunk)
                    {
                        //parallelization option for tests in assembly already is included in args
                        //one CLI console - one assembly with tests
                        var pid = RunTests(args); //the tests are run by VSTest CLI ("dotnet test <dll> <params> ...")
                        if (pid == 0)
                            continue;
                        pids.Add(pid);
                    }

                    if (grpInd == groups.Count - 1 && chInd == chunks.Count - 1)
                        return; //no sense to wait LAST run

                    // we must wait here until all CLI are finish
                    WaitForFinishingProcesses(pids);
                }
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
                Task.Delay(2000);
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
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true
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
