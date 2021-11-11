using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.BanderLog;
using System.Reflection;

/*** INFO
automatic version tagger including Git info - https://github.com/devlooped/GitInfo
semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
the most common format is v0.0 (or just 0.0 is enough)
to change semVer it is nesseccary to create appropriate tag and push it to remote repository
patches'(commits) count starts with 0 again after new tag pushing
For file version format exactly is digit
***/
[assembly: AssemblyFileVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.SemVer.Patch)]

[assembly: AssemblyInformationalVersion(
  ThisAssembly.Git.SemVer.Major + "." +
  ThisAssembly.Git.SemVer.Minor + "." +
  ThisAssembly.Git.SemVer.Patch + "-" +
  ThisAssembly.Git.Branch + "+" +
  ThisAssembly.Git.Commit)]

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
                var (runType, tests) = await _rep.GetRunToTests().ConfigureAwait(false);
                if (runType == RunningType.Nothing)
                    return;
                var args = GetRunArguments(tests);
                RunTests(args); //we need for the test's names here - they runs by CLI ("dotnet test ...")

                _logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Get builds' summary", ex);
            }
        }

        /// <summary>
        /// Get the arguments for running the test by VSTest CLI
        /// </summary>
        /// <param name="tests"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal string GetRunArguments(IList<string> tests)
        {
            // prefix "/C" - is for running in the CMD
            var args = $"/C dotnet test \"{_rep.Options.FilePath}\"";
            if (tests?.Any() != true)
                return args;
            //
            args += " --filter \"";
            for (int i = 0; i < tests.Count; i++)
            {
                string test = tests[i];

                // test case -> just test name. Is it Guanito? No... SpecFlow's test cases contain bracket - so, VSTest break
                var ind = test.IndexOf("("); //after ( the parameters of case followed 
                if (ind != -1)
                    test = test[..ind];
                //
                test = test.Replace(",", "%2C").Replace("\"","\\\"").Replace("!", "\\!"); //need escaping
                //FullyQualifiedName is full type name - for exactly comparing, as =, we need name with namespace
                //TODO: = comparing with real namespaces
                args += $"FullyQualifiedName~.{test}";
                if (i < tests.Count - 1)
                    args += "|";
                else
                    args += "\"";
            }

            args += $" {GetLoggerParameters()} {GetParallelRunParameters()}"; //RunConfiguration must be at the end of args

            if (args.Length > 32767)
                throw new Exception("Argument's length exceeds the maximum. We need improve algorithm (to do some separate runnings)");
            return args;
        }

        internal string GetParallelRunParameters()
        {
            //TODO: depending on the type of test framework: xUnit still disable, others aren't
            //Config - is bad... from Tree after injection?... hmmm...
            return "-- RunConfiguration.DisableParallelization=true";
        }

        internal string GetLoggerParameters()
        {
            return "--logger \"console;verbosity=detailed\"";
        }

        /// <summary>
        /// Run the specified test in arguments by VSTest CLI
        /// </summary>
        /// <param name="args"></param>
        internal void RunTests(string args)
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
