using System.IO;
using Drill4Net.Common;
using Drill4Net.Core.Repository;

namespace Drill4Net.Agent.Kafka.Transmitter
{
    public class TransmitterRepository : AbstractRepository<TransmitterOptions>
    {
        /// <summary>
        /// Gets or sets the testin gtarget which transmitter will be loaded into.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public string Target { get; set; } //TODO: retrieve it from... proxy??

        /*********************************************************************************/

        public TransmitterRepository() : base(CoreConstants.SUBSYSTEM_TRANSMITTER)
        {
            var optHelper = new BaseOptionsHelper<TransmitterOptions>();
            var path = Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_SERVICE_NAME);
            Options = optHelper.ReadOptions(path);

            PrepareLogger();
        }
    }
}
