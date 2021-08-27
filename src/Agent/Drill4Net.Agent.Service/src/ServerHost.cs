using System;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging.Transport;
using Drill4Net.Agent.Messaging.Transport.Kafka;

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
    public class ServerHost : BackgroundService
    {
        private readonly ILogger<ServerHost> _logger;

        /*****************************************************************************/

        public ServerHost(ILogger<ServerHost> logger)
        {
            _logger = logger;
            //
            var appName = CommonUtils.GetAppName();
            var version = FileUtils.GetProductVersion(typeof(AgentServer));
            var title = $"{appName} {version}";
            _logger.LogInformation($"{nameof(ServerHost)} created: {title}");
        }

        /*****************************************************************************/

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //TODO: factory
                AbstractAgentServerRepository rep =
                    new AgentServerKafkaRepository(CoreConstants.SUBSYSTEM_AGENT_SERVER);
                ITargetInfoReceiver targetReceiver = new TargetInfoKafkaReceiver<AgentServerOptions>(rep);
                IPingReceiver pingReceiver = new PingKafkaReceiver<AgentServerOptions>(rep);
                using var server = new AgentServer(rep, targetReceiver, pingReceiver);
                server.ErrorOccured += Server_ErrorOccured;
                _logger.LogInformation($"{nameof(ServerHost)} ready.");

                server.Start();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Server start is failed");
            }
        }

        private void Server_ErrorOccured(bool isFatal, bool isLocal, string message)
        {
            var mess = $"Local: {isLocal} -> {message}";
            if (isFatal)
                Log.Fatal(mess);
            else
                Log.Error(mess);
        }
    }
}
