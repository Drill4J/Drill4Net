using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using Drill4Net.BanderLog;

namespace Drill4Net.Admin.Requester
{
    //ALSO: http://localhost:8090/apidocs/index.html?url=./openapi.json#/

    public class AdminRequester
    {
        private readonly string _url;
        private readonly string _target;
        private readonly string _build;
        private readonly RestClient _client;
        private readonly Logger _logger;

        /*********************************************************************************/

        public AdminRequester(string subsystem, string url, string target, string build)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _build = build ?? throw new ArgumentNullException(nameof(build));

            var logExtras = new Dictionary<string, object> { { "Target", target } };
            _logger = new TypedLogger<AdminRequester>(subsystem, logExtras);

            _url = ResourceManager.CheckUrl(url);
            _client = new RestClient(_url);
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");
        }

        /*********************************************************************************/

        public virtual Task<List<BuildSummary>> GetBuildSummaries(string target = null)
        {
            //http://localhost:8090/api/agents/IHS-bdd/plugins/test2code/builds/summary
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            var request = new RestRequest(ResourceManager.GetSummaryResource(target), Method.GET, DataFormat.Json);
            return GetData<List<BuildSummary>>(request, target, "builds' summaries", "Bad response for builds' summaries retrieving");
        }

        public virtual Task<TestToRunResponse> GetTestToRun(string target = null)
        {
            //https://kb.epam.com/display/EPMDJ/Code+Coverage+plugin+endpoints
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            var request = new RestRequest(ResourceManager.GetTest2RunResource(target), DataFormat.Json);
            return GetData<TestToRunResponse>(request, target, "test to run", "Bad response for tasks to run retrieving");
        }

        private async Task<T> GetData<T>(IRestRequest request, string target, string purpose, string errorMsg)
        {
            IRestResponse response = null;
            for (var i = 0; i < 25; i++)
            {
                response = _client.Get(request);
                //if (response.StatusCode == HttpStatusCode.OK)
                //    break;
                if (response.StatusCode != HttpStatusCode.BadRequest)
                    break;
                var answer = JsonConvert.DeserializeObject<SimpleRestAnswer>(response.Content);
                if (answer?.message?.Contains("not found") == true) //Drill doesn't know about this Target yet
                {
                    _logger.Info($"New target for Drill: {target}");
                    return default;
                }
                _logger.Warning($"Waiting for retrieving {purpose}...");
                await Task.Delay(5000).ConfigureAwait(false);
            }
            if (response == null || response.StatusCode != HttpStatusCode.OK)
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
