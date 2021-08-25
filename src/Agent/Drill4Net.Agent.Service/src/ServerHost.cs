using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging.Transport;
using Drill4Net.Agent.Messaging.Transport.Kafka;

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
            var appName = ServiceUtils.GetAppName();
            var version = ServiceUtils.GetAppVersion();
            var title = $"{appName} {version}";
            _logger.LogInformation($"{nameof(ServerHost)} created: {title}");
        }

        /*****************************************************************************/

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //TODO: factory
                AbstractRepository<MessageReceiverOptions> rep =
                    new MessageReceiverRepository(CoreConstants.SUBSYSTEM_AGENT_SERVER);
                ITargetInfoReceiver targetReceiver = new TargetInfoKafkaReceiver(rep);
                IPingReceiver pingReceiver = new PingKafkaReceiver(rep);
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
