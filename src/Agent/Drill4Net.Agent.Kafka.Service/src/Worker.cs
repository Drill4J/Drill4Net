using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;
using Drill4Net.Agent.Kafka.Transport;

namespace Drill4Net.Agent.Kafka.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        /*****************************************************************************/

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            //
            var appName = ServiceUtils.GetAppName();
            var version = ServiceUtils.GetAppVersion();
            var title = $"{appName} {version}";
            _logger.LogInformation($"Worker created: {title}");
        }

        /*****************************************************************************/

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}

            AbstractRepository<CommunicatorOptions> rep = new KafkaConsumerRepository();
            IKafkaServerReceiver consumer = new KafkaServerReceiver(rep);
            var agent = new CoverageServer(consumer);
            _logger.LogInformation("Worker ready.");

            //TODO: accept stoppingToken
            agent.Start();
        }
    }
}
