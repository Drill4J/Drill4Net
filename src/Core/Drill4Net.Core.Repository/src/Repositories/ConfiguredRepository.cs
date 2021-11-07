using System;
using Drill4Net.Common;
using Drill4Net.Configuration;

namespace Drill4Net.Core.Repository
{
    /// <summary>
    /// Abstract repository for the injection options, retrieving strategy, directories and files, etc
    /// </summary>
    /// <typeparam name="TOptions">Concrete options</typeparam>
    /// <typeparam name="THelper">Helper for manipulating the concrete type of options</typeparam>
    public abstract class ConfiguredRepository<TOptions, THelper> : AbstractRepository<TOptions>
                    where TOptions : AbstractOptions, new()
                    where THelper : BaseOptionsHelper<TOptions>, new()
    {
        /// <summary>
        /// Gets or sets the default config path. Assigned after reading the config file.
        /// </summary>
        /// <value>
        /// The default config path.
        /// </value>
        public string DefaultCfgPath { get; internal set; }

        protected THelper _optHelper;

        /**********************************************************************************/

        protected ConfiguredRepository(string subsystem, string[] args) : this(subsystem, GetArgumentConfigPath(args))
        {
        }

        protected ConfiguredRepository(string subsystem, string cfgPath) : base(subsystem)
        {
            _optHelper = new THelper();

            //options
            if (string.IsNullOrWhiteSpace(cfgPath))
                cfgPath = _optHelper.GetActualConfigPath(CoreConstants.CONFIG_DEFAULT_NAME);
            DefaultCfgPath = cfgPath;
            Options = _optHelper.ReadOptions(cfgPath);

            //logging
            PrepareLogger();
        }

        protected ConfiguredRepository(string subsystem, TOptions opts) : base(subsystem)
        {
            _optHelper = new THelper();
            Options = opts ?? throw new ArgumentNullException(nameof(opts));
            PrepareLogger();
        }
    }
}
