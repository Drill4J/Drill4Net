using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;

namespace Drill4Net.Admin.Requester
{
    //ALSO: http://localhost:8090/apidocs/index.html?url=./openapi.json#/

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

        public virtual Task<List<BuildSummary>> GetBuildSummaries(string target = null)
        {
            //http://localhost:8090/api/agents/IHS-bdd/plugins/test2code/builds/summary
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            var request = new RestRequest(ResourceManager.GetSummaryResource(target), Method.GET, DataFormat.Json);
            return GetData<List<BuildSummary>>(request, "Bad response for build summaries retrieving");
        }

        public virtual Task<TestToRunResponse> GetTestToRun(string target = null)
        {
            //https://kb.epam.com/display/EPMDJ/Code+Coverage+plugin+endpoints
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            var request = new RestRequest(ResourceManager.GetTest2RunResource(target), DataFormat.Json);
            return GetData<TestToRunResponse>(request, "Bad response for tasks to run retrieving");
        }

        private async Task<T> GetData<T>(IRestRequest request, string errorMsg)
        {
            IRestResponse response = null;
            for (var i = 0; i < 5; i++)
            {
                response = _client.Get(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    break;
                await Task.Delay(500).ConfigureAwait(false);
            }
            if (response == null)
                throw new Exception(errorMsg);
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public virtual object GetTestList(string build = null) => GetTestList(build, null);

        public virtual object GetTestList(string build, string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            if (string.IsNullOrWhiteSpace(build))
                build = _build;
            //
            var request = new RestRequest(ResourceManager.GetTestListResource(target, build), Method.GET, DataFormat.Json);
            var a = _client.Get(request);
            // DON'T WORK YET

            return null;
        }
    }
}
