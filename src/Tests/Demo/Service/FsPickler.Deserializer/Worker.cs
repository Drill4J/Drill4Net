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
    /// <summary>
    /// Test serialization process for different sites (for example, OS versions)
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly string FILE_ORIG = Path.Combine("data", "injected.tree");
        private readonly string FILE_REWRITTEN = Path.Combine("data", "injected_rewritten.tree");
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
        ///<param name="pathToFile">Path to tree's file</param>
        /// <returns>Tree of injected solution</returns>
        private InjectedSolution GetTreeFromFile(string pathToFile)
        {
            InjectedSolution tree = null;

            // try to read data to tree's file
            try
            {
                byte[] origBytes = File.ReadAllBytes(pathToFile);
                _logger.LogInformation($"Tree data was read successfully");

                // try to deserialize tree's file
                if (origBytes != null)
                {
                    try
                    {
                        tree = Serializer.FromArray<InjectedSolution>(origBytes);
                        _logger.LogInformation($"Tree data was deserialized successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Tree data is not deserialized: [{FILE_ORIG}].\n{ex}");
                    }
                }
            }
            catch (IOException ex)
            {
                _logger.LogError($"Can't read the tree's file:\n{ex}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:\n{ex}");
            }

            return tree;
        }

        /// <summary>
        /// Save data to tree's file
        /// </summary>
        /// <param name="pathToFile">Path for saving tree's file</param>
        private void SaveTreeToFile(string pathToFile, InjectedSolution tree)
        {
            byte[] serializedTree = null;
            // try to save Tree data to file on current site (for example, OS version)
            try
            {
                serializedTree = Serializer.ToArray<InjectedSolution>(tree);
                _logger.LogInformation($"Tree data was serialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Can't serialize tree data: {ex}");
            }

            //try to write data to tree's file on current site (for example, OS version)
            try
            {
                File.WriteAllBytes(pathToFile, serializedTree);
                _logger.LogInformation($"Tree was saved successfully to {pathToFile}");
            }
            catch (IOException ex)
            {
                _logger.LogError($"Can't write the tree's file:\n{ex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:\n{ex}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //try to get Tree from tree's file generated on another site (for example, OS version)
            _logger.LogInformation($"Getting Tree from tree's file generated on another site [{FILE_ORIG}]...");
            var anoterSiteTree = GetTreeFromFile(FILE_ORIG);

            if (anoterSiteTree != null)
            {
                // view Tree's info
                _logger.LogInformation($"Tree Description {anoterSiteTree.Description}");
                _logger.LogInformation($"Tree Name {anoterSiteTree.Name}");
                _logger.LogInformation($"Tree StartTime {anoterSiteTree.StartTime}");
                _logger.LogInformation($"Tree FinishTime {anoterSiteTree.FinishTime}");
            }

            // try to save deserialized Tree to file on the same site (for example, OS version)
            _logger.LogInformation($"Saving deserialized Tree to file on the same site [{FILE_REWRITTEN}]...");
            SaveTreeToFile(FILE_REWRITTEN, anoterSiteTree);

            // try to get Tree from tree's file generated on the same site (for example, OS version)
            _logger.LogInformation($"Getting Tree from tree's file generated on the same site [{FILE_REWRITTEN}]...");
            GetTreeFromFile(FILE_REWRITTEN);
        }
    }
}
