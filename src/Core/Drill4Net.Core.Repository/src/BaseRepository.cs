using Serilog;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Core.Repository
{
    /// <summary>
    /// Base repository
    /// </summary>
    public abstract class BaseRepository
    {
        /// <summary>
        /// Gets or sets the default config path. Assigned after reading the config file.
        /// </summary>
        /// <value>
        /// The default config path.
        /// </value>
        public string DefaultCfgPath { get; internal set; }

        protected NetSerializer.Serializer _ser;

        /*******************************************************************/

        protected BaseRepository()
        {
            var types = InjectedSolution.GetInjectedTreeTypes();
            _ser = new NetSerializer.Serializer(types);
        }

        /*******************************************************************/

        /// <summary>
        /// Prepares the initialize logger.
        /// </summary>
        public static void PrepareInitLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            Log.Logger = cfg.CreateLogger();
        }
    }
}
