using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Agent.Abstract
{
    public class ContextDispatcher : AbstractContexter
    {
        private readonly SimpleContexter _stdContexter;
        private readonly List<AbstractContexter> _contexters;
        private static readonly Logger _logger;

        /**********************************************************************************/

        public ContextDispatcher(string dir): base(nameof(ContextDispatcher))
        {
            var pluginator = new Pluginator();
            var filter = new SourceFilterOptions
            {
                Excludes = new SourceFilterParams
                {
                    Classes = new List<string>
                    {
                        "Drill4Net.Agent.Abstract.ContextDispatcher",
                        "Drill4Net.Agent.Abstract.SimpleContexter"
                    },
                },
            };
            var ctxTypes = pluginator.GetByInterface(dir, typeof(AbstractContexter), filter);

            _stdContexter = new SimpleContexter();
            _contexters = new List<AbstractContexter>();
            foreach (var contexter in ctxTypes)
            {
                var obj = Activator.CreateInstance(contexter) as AbstractContexter;
                _contexters.Add(obj);
            }
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
                if (ctx != null)
                    return ctx;
            }
            return _stdContexter.GetContextId();
        }
    }
}
