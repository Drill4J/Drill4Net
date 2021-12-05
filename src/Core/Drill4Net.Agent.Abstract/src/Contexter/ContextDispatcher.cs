using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.BanderLog;

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

        public ContextDispatcher(string dir, string subsystem)
        {
            _logger = new TypedLogger<ContextDispatcher>(subsystem);

            if (string.IsNullOrWhiteSpace(dir))
            {
                _logger.Info($"Plugin directory parameter: [{dir}]");
                dir = FileUtils.EntryDir;
            }
            dir = FileUtils.GetFullPath(dir);
            _logger.Info($"Actual plugin directory: [{dir}]");
            Log.Flush();

            _contextBindings = new();
            var pluginator = new TypeFinder();
            //var filter = new SourceFilterOptions
            //{
            //    Excludes = new SourceFilterParams
            //    {
            //        Classes = new List<string>
            //        {
            //            typeof(ContextDispatcher).FullName,
            //            typeof(SimpleContexter).FullName,
            //        },
            //    },
            //};
            //var tstTypes = pluginator.GetBy(TypeFinderMode.Class, dir, typeof(AbstractContexter), filter);
            //dir = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Drill4Net.Agent.Transmitter.NUnit\netstandard2.0\"; //TEST !!!

            //var ctxTypes = pluginator.GetBy(TypeFinderMode.Interface, dir, typeof(IEngineContexter));
            var ctxTypes = pluginator.GetBy(TypeFinderMode.ClassChildren, dir, typeof(AbstractEngineContexter));

            _contexters = new List<AbstractEngineContexter>();
            foreach (var contexter in ctxTypes)
            {
                try
                {
                    var plug = Activator.CreateInstance(contexter) as AbstractEngineContexter;
                    _contexters.Add(plug);
                    _logger.Info($"Plugin added: [{plug.Name}]");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Plugin creation failed: [{contexter.Name}]", ex);
                }
            }

            _stdContexter = new SimpleContexter();
            _logger.Info($"Plugin added: [{nameof(SimpleContexter)}] (standard)");
        }

        /**********************************************************************************/

        public bool RegisterCommand(int command, string data)
        {
            _logger.Debug($"Command: [{command}] -> [{data}]");
            foreach (var ctxr in _contexters)
            {
                if(!ctxr.RegisterCommand(command, data))
                    _logger.Error($"Unknown command: [{command}] -> [{data}]");
            }
            return true;
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
