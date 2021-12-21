// See https://aka.ms/new-console-template for more information
using System.Reflection;
using Drill4Net.Common;
using Drill4Net.Configurator.App;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyFileVersion(CommonUtils.AssemblyFileGitVersion)]
[assembly: AssemblyInformationalVersion(CommonUtils.AssemblyGitVersion)]

var helper = new ConfiguratorOutputHelper();
try
{
    new ConfiguratorInformer(helper).SetTitle();
    var poller = new ConfiguratorPoller(helper);
    poller.Init();
    poller.Start();
}
catch (Exception ex)
{
    helper.WriteMessage(ex.ToString(), ConfiguratorAppConstants.COLOR_ERROR);
}
