using System;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Messaging.Transport;
using Drill4Net.Agent.Messaging.Transport.Kafka;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
// semVer creates an automatic version number based on the combination of a SemVer-named tag/branches
// the most common format is v0.0 (or just 0.0 is enough)
// to change semVer it is nesseccary to create appropriate tag and push it to remote repository
// patches'(commits) count starts with 0 again after new tag pushing

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
        private readonly Logger _logger;
        private readonly AbstractAgentServerRepository _rep;

        /****************************************************************************/

        public ServerHost(/*ILogger<ServerHost> logger*/)
        {
            //TODO: factory
            _rep = new AgentServerKafkaRepository(CoreConstants.SUBSYSTEM_AGENT_SERVER); //...it will be created here
            _logger = new TypedLogger<ServerHost>(_rep.Subsystem);
            //TODO: cfg option whether to use this logger
            //_logger.GetManager().AddSink(logger);

            //TODO: use also logger from ctor
            var appName = CommonUtils.GetAppName();
            var version = FileUtils.GetProductVersion(typeof(AgentServer));
            var title = $"{appName} {version}";
            _logger.Debug($"{nameof(ServerHost)} created: {title}");
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
                _logger.Info($"{nameof(ServerHost)} ready.");

                server.Start();
                _logger.Info($"{nameof(ServerHost)} finished.");
            }
            catch (Exception ex)
            {
                _logger.Fatal("Server's start is failed", ex);
            }
        }

        private void Server_ErrorOccured(IMessageReceiver source, bool isFatal, bool isLocal, string message)
        {
            var mess = $"Source = {source} -> error (local: {isLocal}): {message}";
            if (isFatal)
                _logger.Fatal(mess);
            else
                _logger.Error(mess);
        }
    }
}
