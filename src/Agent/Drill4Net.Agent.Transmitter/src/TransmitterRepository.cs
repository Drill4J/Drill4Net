using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Profiling.Tree;
using Drill4Net.Core.Repository;
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

        public MessagerOptions MessagerOptions { get; set; }

        public string MessagerConfigPath { get; }

        private InjectedSolution _tree;
        private readonly ContextDispatcher _ctxDisp;

        /*********************************************************************************************/

        public TransmitterRepository() : base(CoreConstants.SUBSYSTEM_TRANSMITTER, string.Empty)
        {
            MessagerConfigPath = Path.Combine(FileUtils.GetAssemblyDir(typeof(TransmitterRepository)), CoreConstants.CONFIG_NAME_MIDDLEWARE);
            if (!File.Exists(MessagerConfigPath))
            {
                var err = $"Messager config path is not exists: [{MessagerConfigPath}]";
                Log.Debug(err);
                Log.Flush();
                throw new Exception(err);
            }
            Log.Debug($"Messager config path: [{MessagerConfigPath}]");
            MessagerOptions = GetMessagerOptions();

            _tree = ReadInjectedTree(); //TODO: remove Target's data with "not current version" from the Solution
            TargetName = Options.Target?.Name ?? _tree.Name ?? GenerateTargetName();
            TargetVersion = Options.Target?.Version ?? _tree.ProductVersion ?? FileUtils.GetProductVersion(Assembly.GetCallingAssembly()); //but Calling/EntryDir is BAD! It's version of Test Framework for tests
            TargetSession = GetSession();

            _ctxDisp = new ContextDispatcher(Options.PluginDir, Subsystem);
        }

        /*********************************************************************************************/

        private MessagerOptions GetMessagerOptions()
        {
            var optHelper = new BaseOptionsHelper<MessagerOptions>();
            return optHelper.ReadOptions(MessagerConfigPath);
        }

        private Guid GetSession()
        {
            return Guid.NewGuid();
        }

        public byte[] GetTargetInfo()
        {
            var targetInfo = new TargetInfo
            {
                TargetName = TargetName,
                TargetVersion = TargetVersion,
                SessionUid = TargetSession,
                Options = Options,
                Solution = _tree,
            };
            _tree = null; //in general, this data is no longer needed - we save memory
            return Serializer.ToArray<TargetInfo>(targetInfo);
        }

        /// <summary>
        /// Generates the name of the target if in injected Options doesn't contains one.
        /// </summary>
        /// <returns></returns>
        private string GenerateTargetName()
        {
            var entryType = Assembly.GetEntryAssembly().EntryPoint.DeclaringType.FullName;
            return $"{entryType.Replace(".", "-")} (generated)";
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
