using Serilog;

namespace Drill4Net.Common
{
    /// <summary>
    /// Root level of Repository's hieararchy
    /// </summary>
    public class AbstractRepository<TOptions> where TOptions : AbstractOptions, new()
    {
        /// <summary>
        /// Gets the name of subsystem.
        /// </summary>
        /// <value>
        /// The subsystem.
        /// </value>
        public string Subsystem { get; }

        /// <summary>
        /// Options for the injection
        /// </summary>
        public TOptions Options { get; set; }

        /*********************************************************************************/

        public AbstractRepository(string subsystem)
        {
            Subsystem = subsystem;
        }

        /*********************************************************************************/

        /// <summary>
        /// Prepares the initialize logger.
        /// </summary>
        public static void PrepareInitLogger(string folder = LoggerHelper.LOG_DIR_DEFAULT)
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration(folder);
            Log.Logger = cfg.CreateLogger();
        }
    }
}
