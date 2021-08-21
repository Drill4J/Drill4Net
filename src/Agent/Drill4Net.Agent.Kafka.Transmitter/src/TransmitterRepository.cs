using System;
using System.IO;
using System.Reflection;
using Drill4Net.Common;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    /// <summary>
    /// Repository for the transmitting of retrieved data from the Proxy class
    /// in Target's memory to the real Agent in another (micro)service
    /// </summary>
    /// <seealso cref="Drill4Net.Common.AbstractRepository;Drill4Net.Agent.Kafka.Transmitter.TransmitterOptions;"/>
    public class TransmitterRepository : ConfiguredRepository<AgentOptions, BaseOptionsHelper<AgentOptions>>
    {
        public TransmitterOptions TransmitterOptions { get; set; }

        /// <summary>
        /// Gets or sets the testin gtarget which transmitter will be loaded into.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public string Target { get; set; }

        /// <summary>
        /// Gets the session of Target/Transmitter's Run.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        public Guid Session { get; }

        /*********************************************************************************************/

        public TransmitterRepository(): base(string.Empty, CoreConstants.SUBSYSTEM_TRANSMITTER)
        {
            var optHelper = new BaseOptionsHelper<TransmitterOptions>();
            var path = Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_SERVICE_NAME);
            TransmitterOptions = optHelper.ReadOptions(path);
            Target = Options.Target?.Name ?? GenerateTargetName();
            Session = GetSession();
            PrepareLogger();
        }

        /*********************************************************************************************/

        private Guid GetSession()
        {
            return Guid.NewGuid();
        }

        public byte[] GetTargetInfo()
        {
            var tree = ReadInjectedTree();
            //TODO: remove not current version of Targets from Solution

            var targetInfo = new TargetInfo
            {
                SessionUid = Session,
                Options = Options,
                Solution = tree,
            };

            var bytes = Serializer.ToArray<TargetInfo>(targetInfo);
            return bytes;
        }

        /// <summary>
        /// Generates the name of the target if in injected Options doesn't contains one.
        /// </summary>
        /// <returns></returns>
        private string GenerateTargetName()
        {
            var entryType = Assembly.GetEntryAssembly().EntryPoint.DeclaringType.FullName;
            return $"{Environment.MachineName}-{entryType.Replace(".", "-")} (generated)";
        }
    }
}
