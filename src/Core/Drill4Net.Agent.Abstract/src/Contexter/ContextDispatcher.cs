using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Manager for retrieving current context of probes using contexter plugins.
    /// </summary>
    public class ContextDispatcher : AbstractContexter
    {
        private readonly SimpleContexter _stdContexter;
        private readonly List<AbstractContexter> _contexters;
        private readonly Logger _logger;

        /**********************************************************************************/

        public ContextDispatcher(string dir, string subsystem): base(nameof(ContextDispatcher))
        {
            _logger = new TypedLogger<ContextDispatcher>(subsystem);

            var pluginator = new Pluginator();
            var filter = new SourceFilterOptions
            {
                Excludes = new SourceFilterParams
                {
                    Classes = new List<string>
                    {
                        typeof(ContextDispatcher).FullName,
                        typeof(SimpleContexter).FullName,
                    },
                },
            };
            var ctxTypes = pluginator.GetByInterface(dir, typeof(AbstractContexter), filter);

            _contexters = new List<AbstractContexter>();
            foreach (var contexter in ctxTypes)
            {
                var plug = Activator.CreateInstance(contexter) as AbstractContexter;
                _contexters.Add(plug);
                _logger.Info($"Plugin added: [{plug.Name}]");
            }

            _stdContexter = new SimpleContexter();
            _logger.Info($"Plugin added (standard): [{_stdContexter.Name}]");
        }

        /**********************************************************************************/

        public override bool RegisterCommand(int command, string data)
        {
            _logger.Debug($"Command: [{command}] -> [{data}]");
            foreach (var ctxr in _contexters)
            {
                if(!ctxr.RegisterCommand(command, data))
                    _logger.Error($"Unknown command: [{command}] -> [{data}]");
            }
            return true;
        }

        public override string GetContextId()
        {
            //TODO: dynamic prioritizing !!!
            foreach (var ctxr in _contexters)
            {
                var ctx = ctxr.GetContextId();
                //_logger.Trace($"Contexter: [{ctxr.Name}] -> [{ctx}]");
                if (ctx != null)
                    return ctx;
            }
            //nobody from specific plugins know how to retrieve the current context...
            //we will use the standard contexter
            return _stdContexter.GetContextId();
        }
    }
}
