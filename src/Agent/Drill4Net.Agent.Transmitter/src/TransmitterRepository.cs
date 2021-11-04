using System;
using System.IO;
using System.Reflection;
using Drill4Net.Common;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Messaging;

namespace Drill4Net.Agent.Transmitter
{
    /// <summary>
    /// Repository for the transmitting of retrieved data from the Proxy class
    /// in Target's memory to the real Agent in another (micro)service
    /// </summary>
    public class TransmitterRepository : TreeRepository<AgentOptions, BaseOptionsHelper<AgentOptions>>, ITargetSenderRepository
    {
        public BaseMessageOptions SenderOptions { get; set; }

        /// <summary>
        /// Gets or sets the testing target which transmitter will be loaded into.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public string TargetName { get; set; }

        /// <summary>
        /// Gets the session of Target/Transmitter's Run.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        public Guid TargetSession { get; }

        /*********************************************************************************************/

        public TransmitterRepository() : base(string.Empty, CoreConstants.SUBSYSTEM_TRANSMITTER)
        {
            SenderOptions = GetSenderOptions();
            TargetName = Options.Target?.Name ?? GenerateTargetName();
            TargetSession = GetSession();
        }

        /*********************************************************************************************/

        private BaseMessageOptions GetSenderOptions()
        {
            var optHelper = new BaseOptionsHelper<BaseMessageOptions>();
            var path = Path.Combine(FileUtils.ExecutingDir, CoreConstants.CONFIG_SERVICE_NAME);
           return optHelper.ReadOptions(path);
        }

        private Guid GetSession()
        {
            return Guid.NewGuid();
        }

        public byte[] GetTargetInfo()
        {
            var tree = ReadInjectedTree();
            //TODO: remove Target's data with "not current version" from the Solution

            var targetInfo = new TargetInfo
            {
                TargetName = TargetName,
                SessionUid = TargetSession,
                Options = Options,
                Solution = tree,
            };

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
    }
}
