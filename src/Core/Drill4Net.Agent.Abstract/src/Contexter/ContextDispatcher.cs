using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.TypeFinding;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Manager for retrieving current context of probes using contexter plugins.
    /// </summary>
    public class ContextDispatcher
    {
        private readonly ConcurrentDictionary<string, string> _contextBindings;
        private readonly SimpleContexter _stdContexter;
        private readonly List<AbstractEngineContexter> _contexters;
        private readonly Logger _logger;

        /**********************************************************************************/

        /// <summary>
        /// Constructor for separate Agent Worker
        /// </summary>
        /// <param name="subsystem"></param>
        public ContextDispatcher(string subsystem)
        {
            _logger = new TypedLogger<ContextDispatcher>(subsystem);
            _contexters = new List<AbstractEngineContexter>();
            _contextBindings = new();

            _stdContexter = new SimpleContexter();
            _logger.Info($"Plugin added: [{nameof(SimpleContexter)}] (standard)");
        }

        /// <summary>
        /// Constructor for real Agent located in the Target
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="subsystem"></param>
        public ContextDispatcher(string dir, string subsystem): this(subsystem)
        {
            if (string.IsNullOrWhiteSpace(dir))
            {
                _logger.Debug($"Plugin directory parameter: [{dir}]");
                dir = FileUtils.EntryDir;
            }
            dir = FileUtils.GetFullPath(dir);
            _logger.Info($"Actual plugin directory: [{dir}]");
            Log.Flush();

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
                ctxTypes = pluginator.GetBy(TypeFinderMode.ClassChildren, dir, nameof(AbstractEngineContexter), filter);
                _logger.Debug($"Items found: {ctxTypes.Count}");
                Log.Flush();
            }
            catch (Exception ex)
            {
                _logger.Fatal("Search for contexters' plugin is failed", ex);
                Log.Flush();
                throw;
            }

            //creating the plugins
            foreach (var contexter in ctxTypes)
            {
                var name = contexter.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                //
                try
                {
                    var plug = Activator.CreateInstance(contexter) as AbstractEngineContexter;
                    if (plug == null)
                        continue;
                    _contexters.Add(plug);
                    _logger.Info($"Plugin added: [{name}]");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Plugin creation failed: [{name}]", ex);
                }
            }
        }

        /**********************************************************************************/

        public object RegisterCommand(int command, string data)
        {
            _logger.Debug($"Command: [{command}]");
            foreach (var ctxr in _contexters)
            {
                var (res, answer) = ctxr.RegisterCommand(command, data);
                if (!res)
                {
                    _logger.Error($"Unknown command: [{command}] -> [{data}]");
                }
                else
                {
                    //only one contexter should get the context of test here
                    var curTestCtx = answer as TestCaseContext;
                    if (curTestCtx != null)
                    {
                        _logger.Debug($"Test context: [{data}]");
                        return curTestCtx;
                    }
                }
            }
            return null; //exactly is true
        }

        public string GetContextId()
        {
            var sysCtx = _stdContexter.GetContextId();
            if (_contextBindings.TryGetValue(sysCtx, out string ctx))
                return ctx;

            //TODO: dynamic prioritizing ?!!!
            foreach (var ctxr in _contexters)
            {
                ctx = ctxr.GetContextId();
                //_logger.Trace($"Contexter: [{ctxr.Name}] -> [{ctx}]");
                if (ctx != null)
                    break;
            }

            //nobody from specific plugins know how to retrieve the current context...
            //we will use the standard contexter
            if (ctx == null)
                 ctx = sysCtx;

            _contextBindings.TryAdd(sysCtx, ctx);
            return ctx;
        }
    }
}
