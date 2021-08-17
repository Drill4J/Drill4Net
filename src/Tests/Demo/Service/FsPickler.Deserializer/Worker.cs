using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace FsPickler.Deserializer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        /***********************************************************************************/

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        /***********************************************************************************/

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InjectedSolution tree=null;
            try
            {
                var bytes2 = File.ReadAllBytes(Path.Combine("test_folder", "injected.tree"));
                try
                {
                    tree = Serializer.FromArray<InjectedSolution>(bytes2);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Tree data not serialized: [{System.IO.Path.Combine("test_folder", "injected.tree")}].\n{ex}", DateTimeOffset.Now);
                    
                }

                _logger.LogInformation($"Tree Description {tree?.Description}", DateTimeOffset.Now);
                _logger.LogInformation($"Tree Count {tree?.Count}", DateTimeOffset.Now);
                _logger.LogInformation($"Tree Name {tree.Name}", DateTimeOffset.Now);
                _logger.LogInformation($"Tree StartTime {tree.StartTime}", DateTimeOffset.Now);
                _logger.LogInformation($"Tree FinishTime {tree.FinishTime}", DateTimeOffset.Now);         
            }
            catch (Exception ex)
            {
                _logger.LogError($"Can't deserialize Tree data: [{System.IO.Path.Combine("test_folder", "injected.tree")}] { ex}", DateTimeOffset.Now);
            }
            // 
            try
            {
                var data = Serializer.ToArray<InjectedSolution>(tree);
                File.WriteAllBytes(Path.Combine("test_folder", "injected_rewritten.tree"), data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Can't serialize Tree data:  { ex}", DateTimeOffset.Now);
            }
            //
            var bytes1 = File.ReadAllBytes(Path.Combine("test_folder", "injected_rewrited.tree"));
            try
            {
                tree = Serializer.FromArray<InjectedSolution>(bytes1);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tree data not serialized: [{Path.Combine("test_folder", "injected_rewrited.tree")}].\n{ex}",
                    DateTimeOffset.Now);
            }
        }
    }
}
