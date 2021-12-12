using System;
using System.Reflection;
using System.Threading.Tasks;
using Drill4Net.Common;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyFileVersion(CommonUtils.AssemblyFileGitVersion)]
[assembly: AssemblyInformationalVersion(CommonUtils.AssemblyGitVersion)]

namespace Drill4Net.Agent.Standard.Tester
{
    static class Program
    {
        public static async Task Main()
        {
            try
            {
                TesterConfiguration.SetTitle();
                await TesterStandartAgent.Init().ConfigureAwait(false);
                OutputInfoHelper.PrintTreeInfo(TesterStandartAgent.TreeInfo);
                OutputInfoHelper.PrintMenu();

                TesterStandartAgent.Polling();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            OutputInfoHelper.WriteMessage("Done.", TesterConstants.COLOR_TEXT);
        }
    }
}