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
        private readonly string _build;
        private readonly RestClient _client;

        /****************************************************************************/

        public AdminRequester(string url, string target, string build)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _build = build ?? throw new ArgumentNullException(nameof(build));
            _url = ResourceManager.CheckUrl(url);
            _client = new RestClient(_url);
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");
        }

        /****************************************************************************/

        public virtual List<BuildSummary> GetBuildSummaries(string target = null)
        {
            //http://localhost:8090/api/agents/IHS-bdd/plugins/test2code/builds/summary

            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            var request = new RestRequest(ResourceManager.GetSummaryResource(target), Method.GET, DataFormat.Json);
            var a = _client.Get(request);
            if (a.StatusCode != System.Net.HttpStatusCode.OK)
                return null;
            var summary = JsonConvert.DeserializeObject<List<BuildSummary>>(a.Content);
            //var summary = await _client.GetAsync<List<BuildSummary>>(request) //it is failed on empty member (Summary)
            //    .ConfigureAwait(false);
            return summary;
        }

        public async virtual Task<TestToRunResponse> GetTestToRun(string target = null)
        {
            //https://kb.epam.com/display/EPMDJ/Code+Coverage+plugin+endpoints

            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            var request = new RestRequest(ResourceManager.GetTest2RunResource(target), DataFormat.Json);
            //var a = client.Get(request);
            var run = await _client.GetAsync<TestToRunResponse>(request)
                 .ConfigureAwait(false);
            return run;
        }

        public virtual object GetTestList(string build = null) => GetTestList(null, build);

        public virtual object GetTestList(string target, string build)
        {
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            if (string.IsNullOrWhiteSpace(build))
                build = _build;
            //
            var request = new RestRequest(ResourceManager.GetTestListResource(target, build), Method.GET, DataFormat.Json);
            var a = _client.Get(request);

            return null;
        }
    }
}
