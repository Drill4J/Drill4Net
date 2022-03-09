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
            Log.Debug($"AdminRequester: URL={url}");
            _client = new RestClient(_url);
            _client.ConfigureWebRequest((r) =>
            {
                r.ServicePoint.Expect100Continue = false;
                r.KeepAlive = true;
            });
            //client.Authenticator = new HttpBasicAuthenticator("username", "password");

            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /*********************************************************************************/

        public virtual Task<List<BuildSummary>> GetBuildSummaries(string target = null)
        {
            //http://localhost:8090/api/agents/IHS-bdd/plugins/test2code/builds/summary
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            var resr = ResourceManager.GetSummaryResource(target);
            Log.Trace("GetBuildSummaries: " + resr);
            var request = new RestRequest(resr, Method.GET, DataFormat.Json);
            return GetData<List<BuildSummary>>(request, target, "builds' summaries", "Bad response for builds' summaries retrieving");
        }

        public virtual Task<TestToRunResponse> GetTestsToRun(string target = null)
        {
            //https://kb.epam.com/display/EPMDJ/Code+Coverage+plugin+endpoints
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            var request = new RestRequest(ResourceManager.GetTest2RunResource(target), DataFormat.Json);
            return GetData<TestToRunResponse>(request, target, "tests to run", "Bad response for tasks to run retrieving");
        }

        public virtual Task<AssociatedTestsResponse> GetAssociatedTests(string build = null) => GetAssociatedTests(build, null);

        public async virtual Task<AssociatedTestsResponse> GetAssociatedTests(string build, string target)
        {
            if (string.IsNullOrWhiteSpace(target))
                target = _target;
            if (string.IsNullOrWhiteSpace(build))
                build = _build;
            //
            //http://localhost:8090/api/plugins/test2code/build/tests?agentId=bdd-specflow-xUnit-kafka&buildVersion=0.1.0&type=AGENT
            var request = new RestRequest(ResourceManager.GetAssociatedTestListResource(target, build), Method.GET, DataFormat.Json);
            //var a = _client.Get(request);
            var tests = await GetData<AssociatedTest[]>(request, target, "associated tests", "Bad response for associated tests retrieving");
            var response = new AssociatedTestsResponse();
            response.Tests.AddRange(tests);
            return response;
        }

        private async Task<T> GetData<T>(IRestRequest request, string target, string purpose, string errorMsg)
        {
            IRestResponse response = null;
            for (var i = 0; i < 25; i++)
            {
                response = _client.Get(request);
                //if (response.StatusCode == HttpStatusCode.OK)
                //    break;
                _logger.Trace($"Response: Type={typeof(T).FullName}; IsSuccessful={response?.IsSuccessful}; StatusCode={response?.StatusCode}; ResponseStatus={response?.ResponseStatus}; ErrorMessage=[{response?.ErrorMessage}]");
                if (response.StatusCode != HttpStatusCode.BadRequest && response.StatusCode != 0)
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

            if (response == null)
                throw new Exception(errorMsg + " -> response is null");
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(errorMsg + $" -> HttpStatusCode={response.StatusCode}. Description=[{response.StatusDescription}]. Content: [{response.Content}]");

            return JsonConvert.DeserializeObject<T>(response.Content);
        }

    }
}
