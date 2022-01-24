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
    internal static class Program
    {
        public static async Task Main()
        {
            var helper = new TesterOutputHelper();
            try
            {
                new TesterInformer(helper).SetTitle();
                var commander = new TesterCommander(helper);
                await commander.Init().ConfigureAwait(false);
                helper.PrintTreeInfo(commander.TreeInfo);
                helper.PrintMenu();

                commander.Poll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            helper.WriteMessage("Done.", TesterConstants.COLOR_TEXT);
            Console.ReadKey(true);
        }
    }
}