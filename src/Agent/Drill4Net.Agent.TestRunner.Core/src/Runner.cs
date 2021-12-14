using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
            _rep.Init();
            //
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
                //TODO: depending on the type of test and its launcher, create different test controllers
                var cli = new CliRunner();
                cli.Start(infos, _rep.Options.DefaultParallelRestrict);

                _logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Get CLI run info", ex);
            }
        }
    }
}
