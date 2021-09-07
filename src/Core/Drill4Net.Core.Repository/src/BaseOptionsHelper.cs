using System.IO;
using YamlDotNet.Serialization;
using Drill4Net.Common;
using Drill4Net.Configuration;
using Drill4Net.BanderLog;

namespace Drill4Net.Core.Repository
{
    /// <summary>
    /// Base generic options Helper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseOptionsHelper<T> where T : AbstractOptions, new()
    {
        private readonly IDeserializer _deser;
        private readonly Logger _logger;

        /********************************************************************/

        public BaseOptionsHelper() : this(null) { }

        public BaseOptionsHelper(string subsystem)
        {
            _logger = new TypedLogger<BaseOptionsHelper<T>>(subsystem);

            _deser = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
        }

        /********************************************************************/

        /// <summary>
        /// Tryings to get the actual configuration file path.
        /// </summary>
        /// <returns></returns>
        protected internal string GetActualConfigPath(string configDefaultName)
        {
            var dir = FileUtils.EntryDir;
            var redirectPath = Path.Combine(dir, CoreConstants.CONFIG_REDIRECT_NAME);
            var defName = string.IsNullOrWhiteSpace(configDefaultName) ? CoreConstants.CONFIG_DEFAULT_NAME : configDefaultName;
            if (!File.Exists(redirectPath))
                return Path.Combine(dir, defName);
            Deserializer deser = new();
            var cfg = File.ReadAllText(redirectPath);
            var redirect = deser.Deserialize<RedirectData>(cfg);
            var path = redirect?.Path;
            if (!path.EndsWith(".yml"))
                path += ".yml";
            return FileUtils.GetFullPath(path);
        }

        /// <summary>
        /// Reads the options by specified file path.
        /// </summary>
        /// <param name="path">The fike path.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">$"Options file not found: [{path}]</exception>
        public T ReadOptions(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Options file not found: [{path}]");
            _logger.Debug($"Reading config: [{path}]");
            var cfg = File.ReadAllText(path);
            var opts = _deser.Deserialize<T>(cfg);
            _logger.Debug("Config deserialized.");
            PostProcess(opts);
            _logger.Debug("Config prepared.");
            return opts;
        }

        /// <summary>
        /// Post-processing for Options after loading it,
        /// override if needed in children
        /// </summary>
        /// <param name="opts"></param>
        protected virtual void PostProcess(T opts) { }
    }
}
