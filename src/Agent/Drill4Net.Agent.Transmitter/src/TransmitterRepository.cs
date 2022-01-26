using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Agent.Abstract;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Agent.Transmitter
{
    /// <summary>
    /// Repository for the transmitting of retrieved data from the Proxy class
    /// in Target's memory to the real Agent in another (micro)service
    /// </summary>
    public class TransmitterRepository : TreeRepository<AgentOptions, BaseOptionsHelper<AgentOptions>>, ITargetedInfoSenderRepository
    {
        /// <summary>
        /// Gets or sets the testing target which transmitter will be loaded into.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public string TargetName { get; set; }

        public string TargetVersion { get; set; }

        /// <summary>
        /// Gets the session of Target/Transmitter's Run.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        public Guid TargetSession { get; }

        public string Directory { get; }

        public MessagerOptions MessagerOptions { get; set; }

        public string MessagerConfigPath { get; }

        private InjectedSolution _tree;
        private readonly ContextDispatcher _ctxDisp;
        private Logger _logger;

        /*********************************************************************************************/

        public TransmitterRepository() : base(CoreConstants.SUBSYSTEM_TRANSMITTER, string.Empty)
        {
            _logger = new TypedLogger<TransmitterRepository>(CoreConstants.SUBSYSTEM_TRANSMITTER);

            //messager options
            Directory = Path.GetDirectoryName(typeof(TransmitterRepository).Assembly.Location);
            MessagerConfigPath = Path.Combine(Directory, CoreConstants.CONFIG_NAME_MIDDLEWARE);
            if (!File.Exists(MessagerConfigPath))
            {
                var err = $"Messager config path is not exists: [{MessagerConfigPath}]";
                Log.Fatal(err);
                Log.Flush();
                throw new Exception(err);
            }
            Log.Debug($"Messager config path: [{MessagerConfigPath}]");
            MessagerOptions = GetMessagerOptions();

            _tree = ReadInjectedTree(); //TODO: remove "not current version framework" data from the tree

            TargetName = Options.Target?.Name ?? _tree.Name ?? TrySearchTargetName(); //don't relocate TrySearchTargetName method to the InjectedSolution, it's local specific
            var target = Options.Target;
            TargetVersion = target?.Version ?? _tree.SearchProductVersion(target?.VersionAssemblyName) ?? FileUtils.GetProductVersion(Assembly.GetCallingAssembly()); //but Calling/EntryDir is BAD! It's version of Test Framework for tests
            TargetSession = GetSession();

            Log.Flush();
            _ctxDisp = new ContextDispatcher(Options.PluginDir, Subsystem); //IEngineContexters plugins
        }

        /*********************************************************************************************/

        private MessagerOptions GetMessagerOptions()
        {
            var optHelper = new BaseOptionsHelper<MessagerOptions>(Subsystem);
            return optHelper.ReadOptions(MessagerConfigPath);
        }

        private Guid GetSession()
        {
            return Guid.NewGuid();
        }

        public byte[] GetTargetInfo()
        {
            var entryAsm = Assembly.GetEntryAssembly()?.Location;
            var targetInfo = new TargetInfo
            {
                TargetName = TargetName,
                TargetVersion = TargetVersion,
                TargetDir = FileUtils.EntryDir, //some directory will be here anyway
                TargetAssembly = entryAsm != null ? Path.GetFileName(entryAsm) : null, //Null happens for some targets by this way
                Session = TargetSession,
                Options = Options,
                Tree = _tree, //excess monikers will be deleted in the Agent
            };
            _logger.Debug($"Target info: {targetInfo}");

            _tree = null; //in general, this data is no longer needed - we save memory
            return Serializer.ToArray<TargetInfo>(targetInfo);
        }

        /// <summary>
        /// Generates the name of the target if in injected Options doesn't contains one.
        /// </summary>
        /// <returns></returns>
        internal string TrySearchTargetName()
        {
            var entryType = Assembly.GetEntryAssembly()?.EntryPoint?.DeclaringType?.FullName;
            if (entryType == null) // over-reinsurance
                //TODO: implenet more effective algorithm for first assembly
                entryType = _tree.GetAllAssemblies().First().GetAllTypes().First(a => !a.IsCompilerGenerated)?.BusinessType ?? "unknown";
            return $"$_{entryType.Replace(".", "-")}";
        }

        internal IEnumerable<string> GetSenderCommandTopics()
        {
            return MessagingUtils.FilterCommandTopics(MessagerOptions.Sender?.Topics);
        }

        public string GetContextId()
        {
            return _ctxDisp.GetContextId();
        }

        public void RegisterCommandByPlugins(int command, string data)
        {
            _ctxDisp.RegisterCommand(command, data);
        }
    }
}
