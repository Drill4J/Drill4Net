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
        private readonly string FILE_ORIG = Path.Combine("test_folder", "injected.tree");
        private readonly string FILE_REWRITTEN = Path.Combine("test_folder", "injected_rewrited.tree");
        private readonly ILogger<Worker> _logger;

        /***********************************************************************************/

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        /***********************************************************************************/

        /// <summary>
        /// Get Tree from tree's file
        /// </summary>
        private InjectedSolution GetTreeFromFile(string pathToFile)
        {
            InjectedSolution tree = null;

            // try to read data to tree's file
            try
            {
                byte[] origBytes = File.ReadAllBytes(pathToFile);
                _logger.LogInformation($"Tree data was read successfully", DateTimeOffset.Now);

                // try to deserialize tree's file
                if (origBytes != null)
                {
                    try
                    {
                        tree = Serializer.FromArray<InjectedSolution>(origBytes);
                        _logger.LogInformation($"Tree data was deserialized successfully", DateTimeOffset.Now);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Tree data is not deserialized: [{FILE_ORIG}].\n{ex}", DateTimeOffset.Now);
                    }
                }
            }
            catch (IOException ex)
            {
                _logger.LogError($"Can't read the tree's file:\n{ex}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:\n{ex}");
            }

            return tree;
        }

        /// <summary>
        /// try to write data to tree's file
        /// </summary>
        private void SaveTreeToFile(string pathToFile, InjectedSolution tree)
        {

            byte[] serializedTree = null;
            // try to save Tree data to file on current site (for example, OS version)
            try
            {
                serializedTree = Serializer.ToArray<InjectedSolution>(tree);
                _logger.LogInformation($"Tree data was serialized successfully", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Can't serialize tree data: {ex}", DateTimeOffset.Now);
            }

            //try to write data to tree's file on current site (for example, OS version)
            try
            {
                File.WriteAllBytes(pathToFile, serializedTree);
                _logger.LogInformation($"Tree was saved successfully to {pathToFile}", DateTimeOffset.Now);
            }
            catch (IOException ex)
            {
                _logger.LogError($"Can't write the tree's file:\n{ex}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:\n{ex}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //try to get Tree from tree's file generated on another site (for example, OS version)
            _logger.LogInformation($"Getting Tree from tree's file generated on another site [{FILE_ORIG}]...", DateTimeOffset.Now);
            var anoterSiteTree = GetTreeFromFile(FILE_ORIG);

            if (anoterSiteTree != null)
            {
                // view Tree's info
                _logger.LogInformation($"Tree Description {anoterSiteTree.Description}", DateTimeOffset.Now);
                _logger.LogInformation($"Tree Name {anoterSiteTree.Name}", DateTimeOffset.Now);
                _logger.LogInformation($"Tree StartTime {anoterSiteTree.StartTime}", DateTimeOffset.Now);
                _logger.LogInformation($"Tree FinishTime {anoterSiteTree.FinishTime}", DateTimeOffset.Now);
            }

            // try to save deserialized Tree to file on the same site (for example, OS version)
            _logger.LogInformation($"Saving deserialized Tree to file on the same site [{FILE_REWRITTEN}]...", DateTimeOffset.Now);
            SaveTreeToFile(FILE_REWRITTEN, anoterSiteTree);

            // try to get Tree from tree's file generated on the same site (for example, OS version)
            _logger.LogInformation($"Getting Tree from tree's file generated on the same site [{FILE_REWRITTEN}]...", DateTimeOffset.Now);
            GetTreeFromFile(FILE_REWRITTEN);
        }
    }
}
