using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FsPickler.Deserializer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
          
                InjectedSolution tree=null;
                try
                {
                    
                    var bytes2 = File.ReadAllBytes(System.IO.Path.Combine("test_folder", "injected.tree"));
                    try
                    {
                        tree = Serializer.FromArray<InjectedSolution>(bytes2);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Tree data not serialized: [{System.IO.Path.Combine("test_folder", "injected.tree")}].\n{ex}", DateTimeOffset.Now);
                        
                    }
                    _logger.LogInformation($"Tree Description {tree?.Description}", DateTimeOffset.Now);
                    _logger.LogInformation($"Tree Count {tree?.Count.ToString()}", DateTimeOffset.Now);
                    _logger.LogInformation($"Tree Name {tree.Name}", DateTimeOffset.Now);
                    _logger.LogInformation($"Tree StartTime {tree.StartTime.ToString()}", DateTimeOffset.Now);
                    _logger.LogInformation($"Tree FinishTime {tree.FinishTime.ToString()}", DateTimeOffset.Now);                
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Can't deserialize Tree data: [{System.IO.Path.Combine("test_folder", "injected.tree")}] { ex}", DateTimeOffset.Now);
                }
                try
                {
                    var data = Serializer.ToArray<InjectedSolution>(tree);
                    File.WriteAllBytes(
                        System.IO.Path.Combine("test_folder", "injected_rewritten.tree"), data);
                }

                catch (Exception ex)
                {
                    _logger.LogError($"Can't serialize Tree data:  { ex}", DateTimeOffset.Now);
                }
                tree = null;
                var bytes1 = File.ReadAllBytes(System.IO.Path.Combine("test_folder", "injected_rewrited.tree"));
                try
                {
                    tree = Serializer.FromArray<InjectedSolution>(bytes1);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Tree data not serialized: [{System.IO.Path.Combine("test_folder", "injected_rewrited.tree")}].\n{ex}", DateTimeOffset.Now);

                }
        }
    }
}
