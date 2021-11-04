using System;
using System.Linq;
using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Agent.Abstract
{
    public class ContextDispatcher : AbstractContexter
    {
        private readonly SimpleContexter _stdContexter;
        private readonly List<AbstractContexter> _contexters;

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

        public override void RegisterCommand(int command, string data)
        {

            foreach (var ctxr in _contexters)
                ctxr.RegisterCommand(command, data);
        }

        public override string GetContextId()
        {
            return "NOT_IMPLEMENTED";
        }
    }
}
