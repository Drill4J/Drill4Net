using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyInformationalVersion(
      ThisAssembly.Git.SemVer.Major + "." +
      ThisAssembly.Git.SemVer.Minor + "." +
      ThisAssembly.Git.SemVer.Patch + "-" +
      ThisAssembly.Git.Branch + "+" +
      ThisAssembly.Git.Commit)]

namespace Drill4Net.Agent.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ServerHost>();
                });
    }
}
