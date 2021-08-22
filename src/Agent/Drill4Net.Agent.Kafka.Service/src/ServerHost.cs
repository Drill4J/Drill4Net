using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Service
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
                AbstractRepository<MessageReceiverOptions> rep = new KafkaReceiverRepository();
                ITargetInfoReceiver receiver = new TargetInfoReceiver(rep);
                var server = new CoverageServer(rep, receiver);
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
