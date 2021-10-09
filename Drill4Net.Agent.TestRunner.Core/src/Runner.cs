using System;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using RestSharp;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
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
            _logger.Debug("Running...");
            var url = _rep.GetUrl();
            var client = new RestClient(url);
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");
            List<BuildSummary> summary = null;

            try
            {
                var request = new RestRequest(_rep.GetSummaryResource(), DataFormat.Json);
                //var a = client.Get(request);
                summary = await client.GetAsync<List<BuildSummary>>(request);

                _logger.Debug("Finished");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Get summary", ex);
            }
        }
    }
}
