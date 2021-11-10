using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;

namespace Drill4Net.Admin.Requester
{
    public class AdminRequester
    {
        private readonly string _url;
        private readonly string _target;
        private readonly RestClient _client;

        /****************************************************************************/

        public AdminRequester(string url, string target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _url = ResourceManager.CheckUrl(url);
            _client = new RestClient(_url);
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");
        }

        /****************************************************************************/

        public virtual List<BuildSummary> GetBuildSummaries()
        {
            //http://localhost:8090/api/agents/IHS-bdd/plugins/test2code/builds/summary

            var request = new RestRequest(ResourceManager.GetSummaryResource(_target), Method.GET, DataFormat.Json);
            var a = _client.Get(request);
            if (a.StatusCode != System.Net.HttpStatusCode.OK)
                return null;
            var summary = JsonConvert.DeserializeObject<List<BuildSummary>>(a.Content);
            //var summary = await _client.GetAsync<List<BuildSummary>>(request) //it is failed on empty member (Summary)
            //    .ConfigureAwait(false);
            return summary;
        }

        public async virtual Task<TestToRunResponse> GetTestToRun()
        {
            //https://kb.epam.com/display/EPMDJ/Code+Coverage+plugin+endpoints
            var request = new RestRequest(ResourceManager.GetTest2RunResource(_target), DataFormat.Json);
            //var a = client.Get(request);
            var run = await _client.GetAsync<TestToRunResponse>(request)
                 .ConfigureAwait(false);
            return run;
        }
    }
}
