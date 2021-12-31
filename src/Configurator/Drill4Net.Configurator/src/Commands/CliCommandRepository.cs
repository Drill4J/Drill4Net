using System;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.TypeFinding;

namespace Drill4Net.Configurator
{
    public class CliCommandRepository
    {
        public List<AbstractCliCommand> Commands { get; }

        private readonly ConfiguratorRepository _rep;
        private readonly Logger _logger;

        /***********************************************************************************/

        public CliCommandRepository(ConfiguratorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            Commands = SearchCommands();
            _logger = new TypedLogger<CliCommandRepository>(rep.Subsystem);
        }

        /***********************************************************************************/

        internal List<AbstractCliCommand> SearchCommands()
        {
            var res = new List<AbstractCliCommand>();

            //search the plugins
            var pluginator = new TypeFinder();
            var filter = new SourceFilterOptions
            {
                Excludes = new SourceFilterParams
                {
                    Files = new List<string>
                    {
                        "reg:.resources.dll$",
                    },
                },
            };
            List<Type> ctxTypes;
            try
            {
                //search in local dir
                ctxTypes = pluginator.GetBy(TypeFinderMode.ClassChildren, FileUtils.EntryDir, typeof(AbstractCliCommand), filter);
            }
            catch (Exception ex)
            {
                _logger.Fatal("Search for CLI commands is failed", ex);
                Log.Flush();
                throw;
            }

            // creating the command
            foreach (var type in ctxTypes)
            {
                var name = type.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                //
                try
                {
                    if (Activator.CreateInstance(type, new object[] { _rep }) is not AbstractCliCommand obj)
                        continue;
                    res.Add(obj);
                    _logger.Info($"Command added: [{name}]");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Command's creation failed: [{name}]", ex);
                }
            }
            return res;
        }

    }
}
