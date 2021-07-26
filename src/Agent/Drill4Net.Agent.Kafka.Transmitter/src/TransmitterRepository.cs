using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

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

        /*********************************************************************************/

        public TransmitterRepository(): base(string.Empty, CoreConstants.SUBSYSTEM_TRANSMITTER)
        {
            var optHelper = new BaseOptionsHelper<TransmitterOptions>();
            var path = Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_SERVICE_NAME);
            TransmitterOptions = optHelper.ReadOptions(path);
            Target = Options.Target.Name;

            PrepareLogger();
        }
    }
}
