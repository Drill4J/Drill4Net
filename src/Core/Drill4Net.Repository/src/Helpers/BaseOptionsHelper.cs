using System;
using System.IO;
using YamlDotNet.Serialization;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Configuration;

namespace Drill4Net.Repository
{
    public class BaseOptionsHelper
    {
        protected readonly ISerializer _ser;
        protected readonly IDeserializer _deser;
        protected readonly Logger _logger;

        /********************************************************************/

        public BaseOptionsHelper(string subsystem)
        {
            _logger = new TypedLogger<BaseOptionsHelper>(subsystem);

            _ser = new YamlDotNet.Serialization.Serializer();
            _deser = new DeserializerBuilder()
                .IgnoreUnmatchedProperties() //TODO: from cfg only if permitted
                .Build();
        }

        /********************************************************************/

        public string GetActualConfigPath()
        {
            return GetActualConfigPath(null, null);
        }

        /// <summary>
        /// Tryings to get the actual configuration file path.
        /// </summary>
        /// <returns></returns>
        public string GetActualConfigPath(string dir, string configDefaultName = null)
        {
            var redirectCfgPath = CreateRedirectConfigPath(dir); //possible redirect
            var defName = string.IsNullOrWhiteSpace(configDefaultName) ? CoreConstants.CONFIG_NAME_DEFAULT : configDefaultName;
            if (!File.Exists(redirectCfgPath))
                return Path.Combine(dir, defName);
            //
            var redirect = ReadRedirectData(redirectCfgPath);
            var actualPath = redirect?.Path;
            if (actualPath == null)
                throw new Exception($"Redirect file is wrong: [{redirectCfgPath}]");
            //
            if (!actualPath.EndsWith(".yml"))
                actualPath += ".yml";
            actualPath = FileUtils.GetFullPath(actualPath);
            _logger.Info($"Actual config path is defined: [{actualPath}]");
            Log.Flush();
            return actualPath;
        }

        public string CreateRedirectConfigPath(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                dir = FileUtils.EntryDir;
            return Path.Combine(dir, CoreConstants.CONFIG_NAME_REDIRECT);
        }

        public RedirectData ReadRedirectData(string redirectCfgPath)
        {
            if (string.IsNullOrWhiteSpace(redirectCfgPath))
                throw new ArgumentNullException(nameof(redirectCfgPath));
            //
            var cfg = File.ReadAllText(redirectCfgPath);
            return _deser.Deserialize<RedirectData>(cfg);
        }

        public void WriteRedirectData(RedirectData data, string redirectCfgPath)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(redirectCfgPath))
                throw new ArgumentNullException(nameof(redirectCfgPath));
            //
            var text = _ser.Serialize(data);
            File.WriteAllText(redirectCfgPath, text);
        }
    }

    /***************************************************************************************************/

    /// <summary>
    /// Base generic options Helper
    /// </summary>
    /// <typeparam name="TOpts"></typeparam>
    public class BaseOptionsHelper<TOpts> : BaseOptionsHelper where TOpts : AbstractOptions, new()
    {
        public BaseOptionsHelper() : this(null) { }

        public BaseOptionsHelper(string subsystem): base(subsystem)
        {
        }

        /***********************************************************************/

        /// <summary>
        /// Reads the options by specified file path.
        /// </summary>
        /// <param name="path">The fike path.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">$"Options file not found: [{path}]</exception>
        public TOpts ReadOptions(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Options file not found: [{path}]");
            _logger.Debug($"Reading config: [{path}]");
            var cfg = File.ReadAllText(path);
            var opts = _deser.Deserialize<TOpts>(cfg);
            _logger.Debug("Config deserialized.");
            PostProcess(opts);
            _logger.Debug("Config prepared.");
            return opts;
        }

        /// <summary>
        /// Writes the options by specified file path.
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="path">The fike path.</param>
        /// <returns></returns>
        public void WriteOptions(AbstractOptions opts, string path)
        {
            if (opts == null)
                throw new ArgumentNullException(nameof(opts));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));
            //
            _logger.Debug($"Writing config {opts.GetType().Name}: [{path}]");
            var text = _ser.Serialize(opts);
            File.WriteAllText(path, text);
            _logger.Debug("Config is saved.");
        }

        /// <summary>
        /// Post-processing for Options after loading it,
        /// override if needed in children
        /// </summary>
        /// <param name="opts"></param>
        protected virtual void PostProcess(TOpts opts) { }
    }
}
