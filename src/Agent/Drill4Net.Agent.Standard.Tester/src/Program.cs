using System;
using System.Reflection;
using System.Threading.Tasks;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyFileVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.SemVer.Patch)]

[assembly: AssemblyInformationalVersion(
      ThisAssembly.Git.SemVer.Major + "." +
      ThisAssembly.Git.SemVer.Minor + "." +
      ThisAssembly.Git.SemVer.Patch + "-" +
      ThisAssembly.Git.Branch + "+" +
      ThisAssembly.Git.Commit)]

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