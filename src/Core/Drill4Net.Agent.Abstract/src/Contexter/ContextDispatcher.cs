﻿using System;
using System.IO;
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

            //search plugin
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
                //CommonUtils.WriteTempLog("Ready to get the plugins");
                ctxTypes = pluginator.GetBy(TypeFinderMode.ClassChildren, dir, typeof(AbstractEngineContexter), filter);
                //CommonUtils.WriteTempLog($"Found plugins: {ctxTypes.Count}");
            }
            catch (Exception ex)
            {
                var err = "Search for contexters' plugin is failed";
                //CommonUtils.WriteTempLog($"{err}: {ex}");
                _logger.Fatal(err, ex);
                Log.Flush();
                throw;
            }

            //creating plugin
            _contexters = new List<AbstractEngineContexter>();
            foreach (var contexter in ctxTypes)
            {
                var name = contexter.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                //CommonUtils.WriteTempLog($"Creating: {name}");
                //
                try
                {
                    var plug = Activator.CreateInstance(contexter) as AbstractEngineContexter;
                    _contexters.Add(plug);
                    _logger.Info($"Plugin added: [{name}]");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Plugin creation failed: [{name}]", ex);
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
