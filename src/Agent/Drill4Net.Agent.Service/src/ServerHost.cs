using System;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;
using Drill4Net.BanderLog;
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
        private readonly AbstractAgentServerRepository _rep;

        /****************************************************************************/

        public ServerHost(ILogger<ServerHost> logger)
        {
            //TODO: factory
            _rep = new AgentServerKafkaRepository(CoreConstants.SUBSYSTEM_AGENT_SERVER); //...it will be created here

            //TODO: use also logger from ctor
            var appName = CommonUtils.GetAppName();
            var version = FileUtils.GetProductVersion(typeof(AgentServer));
            var title = $"{appName} {version}";
            Log.Debug($"{nameof(ServerHost)} created: {title}");
        }

        /*****************************************************************************/

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                ITargetInfoReceiver targetReceiver = new TargetInfoKafkaReceiver<AgentServerOptions>(_rep);
                IPingReceiver pingReceiver = new PingKafkaReceiver<AgentServerOptions>(_rep);
                using var server = new AgentServer(_rep, targetReceiver, pingReceiver);
                server.ErrorOccured += Server_ErrorOccured;
                Log.Info($"{nameof(ServerHost)} ready.");

                server.Start();
            }
            catch (Exception ex)
            {
                Log.Fatal("Server's start is failed", ex);
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
