using System;

namespace Drill4Net.Admin.Requester
{
    public static class ResourceManager
    {
        public static string CheckUrl(string url)
        {
            //{url}/api/agents/{Id}/plugins/test2code/builds/summary
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
    }
}
