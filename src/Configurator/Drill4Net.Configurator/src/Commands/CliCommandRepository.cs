using System;
using System.Collections.Generic;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.TypeFinding;

namespace Drill4Net.Configurator
{
    /// <summary>
    /// Repository for Configurator's <see cref="AbstractConfiguratorCommand"/>
    /// </summary>
    public class CliCommandRepository
    {
        /// <summary>
        /// Dictionary fo Configurator's commands, where key is commandId
        /// </summary>
        public Dictionary<string, AbstractCliCommand> Commands { get; }

        private readonly ConfiguratorRepository _rep;
        private readonly Logger _logger;

        /***********************************************************************************/

        public CliCommandRepository(ConfiguratorRepository rep)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _logger = new TypedLogger<CliCommandRepository>(rep.Subsystem);
            Commands = SearchCommands();
        }

        /***********************************************************************************/

        internal Dictionary<string, AbstractCliCommand> SearchCommands()
        {
            var res = new Dictionary<string, AbstractCliCommand>();

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
                ctxTypes = pluginator.GetBy(TypeFinderMode.Attribute, FileUtils.EntryDir, typeof(CliCommandAttribute), filter);
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
                    AbstractCliCommand? cmd = null;
                    if(type.IsSubclassOf(typeof(AbstractConfiguratorCommand)))
                        cmd = (AbstractConfiguratorCommand)Activator.CreateInstance(type, new object[] { _rep });
                    else
                    if (type.BaseType?.FullName == typeof(AbstractCliCommand).FullName)
                        cmd = (AbstractCliCommand)Activator.CreateInstance(type);
                    if (cmd == null)
                        continue;
                    //
                    var id = cmd.ContextId;
                    if (res.ContainsKey(id))
                    {
                        _logger.Warning($"Command already added: [{name}]");
                        continue;
                    }
                    res.Add(id, cmd);
                    _logger.Info($"Command added: [{name}]");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Command's creation failed: [{name}]", ex);
                }
            }
            return res;
        }

        public AbstractCliCommand GetCommand(string id)
        {
            return string.IsNullOrWhiteSpace(id) || !Commands.ContainsKey(id)
                ? new NullCliCommand() :
                Commands[id];
        }
    }
}
