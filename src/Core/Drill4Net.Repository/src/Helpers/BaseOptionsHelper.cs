using System;
using System.IO;
using YamlDotNet.Serialization;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Configuration;

namespace Drill4Net.Repository
{
    /// <summary>
    /// Base generic options Helper
    /// </summary>
    /// <typeparam name="TOpts"></typeparam>
    public class BaseOptionsHelper<TOpts> where TOpts : AbstractOptions, new()
    {
        private readonly ISerializer _ser;
        private readonly IDeserializer _deser;
        private readonly Logger _logger;

        /********************************************************************/

        public BaseOptionsHelper() : this(null) { }

        public BaseOptionsHelper(string subsystem)
        {
            _logger = new TypedLogger<BaseOptionsHelper<TOpts>>(subsystem);

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
            if(string.IsNullOrWhiteSpace(dir))
                dir = FileUtils.EntryDir;
            var redirectPath = Path.Combine(dir, CoreConstants.CONFIG_NAME_REDIRECT); //possible redirect
            var defName = string.IsNullOrWhiteSpace(configDefaultName) ? CoreConstants.CONFIG_NAME_DEFAULT : configDefaultName;
            if (!File.Exists(redirectPath))
                return Path.Combine(dir, defName);
            Deserializer deser = new();
            var cfg = File.ReadAllText(redirectPath);
            var redirect = deser.Deserialize<RedirectData>(cfg);
            var path = redirect?.Path;
            if (!path.EndsWith(".yml"))
                path += ".yml";
            var actualPath = FileUtils.GetFullPath(path);
            _logger.Info($"Actual config path is defined: [{actualPath}]");
            Log.Flush();
            return actualPath;
        }

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
