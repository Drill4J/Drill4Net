using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Serilog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard
{
    public class StandardAgent : AbstractAgent
    {
        private static readonly ConcurrentDictionary<int, Dictionary<string, List<string>>> _clientPoints;
        private static readonly Dictionary<string, InjectedSimpleEntity> _pointMap;
        private static readonly Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> _parentMap;
        
        /*****************************************************************************/

        static StandardAgent()
        {
            _clientPoints = new ConcurrentDictionary<int, Dictionary<string, List<string>>>();
            PrepareLogger();
            Log.Debug("Initializing...");

            try
            {
                //rep
                var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_TESTS_NAME);
                var rep = new InjectorRepository(cfg_path);
                var opts = rep.Options;

                //tree info
                var targetDir = opts.Destination.Directory;
                var treePath = rep.GenerateTreeFilePath(targetDir);
                var tree = rep.ReadInjectedTree(treePath);
                _parentMap = tree.CalcParentMap();
                _pointMap = tree.CalcPointMap(_parentMap);

                Log.Debug("Initialized.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error of TestingProfiler initializing");
            }
        }

        /*****************************************************************************/
        
        // ReSharper disable once MemberCanBePrivate.Global
        public static void RegisterStatic(string data)
        {
            try
            {
                #region Checks
                if (string.IsNullOrWhiteSpace(data))
                {
                    Log.Error("Data is empty");
                    return;
                }
                //
                var ar = data.Split('^');
                if (ar.Length < 4)
                {
                    Log.Error($"Bad format of input: {data}");
                    return;
                }
                #endregion

                var probeUid = ar[0];
                var asmName = ar[1];
                //var funcName = ar[2];
                var probe = ar[3];

                var businessMethod = GetBusinessMethodName(probeUid);
                if(businessMethod != null)
                    AddPoint(asmName, businessMethod, $"{probeUid}:{probe}"); //in the prod profiler only uid needed
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{data}");
            }
        }

        public override void Register(string data)
        {
            RegisterStatic(data);
        }
        
        internal static string GetBusinessMethodName(string probeUid)
        {
            if (_pointMap == null)
                return null;
            if (!_pointMap.ContainsKey(probeUid))
                return null;
            if (_pointMap[probeUid] is not CrossPoint point)
                return null;
            if (_parentMap[point] is not InjectedMethod method)
                return null;
            return method.BusinessMethod;
        }
        
        public static void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\..\"), $"{nameof(StandardAgent)}.log"));
            Log.Logger = cfg.CreateLogger();
        }
    }
}