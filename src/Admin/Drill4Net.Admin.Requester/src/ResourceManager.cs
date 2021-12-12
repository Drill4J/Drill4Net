using System;

namespace Drill4Net.Admin.Requester
{
    public static class ResourceManager
    {
        public static string CheckUrl(string url)
        {
            if(string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            if (!url.StartsWith("http"))
                url = "http://" + url; //TODO: check for https
            return url;
        }

        public static string GetSummaryResource(string target)
        {
            return $"api/agents/{target}/plugins/test2code/builds/summary";
        }

        public static string GetTest2RunResource(string target)
        {
            return $"api/agents/{target}/plugins/test2code/data/tests-to-run";
        }

        public static string GetTestListResource(string target, string build)
        {
            //http://localhost:8090/api/plugins/test2code/build/tests?agentId=IHS-bdd&buildVersion=0.8.66-main+0a5448c&type=AGENT
            return $"api/plugins/test2code/build/tests?agentId={target}&buildVersion={build}&type=AGENT";
        }
    }
}
