using System.IO;
using Serilog;
using YamlDotNet.Serialization;
using Drill4Net.Common;

namespace Drill4Net.Core.Repository
{
    /// <summary>
    /// Base generic options Helper
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseOptionsHelper<T> where T : BaseOptions, new()
    {
        private readonly Deserializer _deser;

        /********************************************************************/

        public BaseOptionsHelper()
        {
            _deser = new Deserializer();
        }

        /********************************************************************/

        protected internal string GetActualConfigPath()
        {
            var redirectPath = Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_REDIRECT_NAME);
            if (!File.Exists(redirectPath))
                return Path.Combine(FileUtils.GetExecutionDir(), CoreConstants.CONFIG_DEFAULT_NAME);
            Deserializer deser = new();
            var cfg = File.ReadAllText(redirectPath);
            var redirect = deser.Deserialize<RedirectOptions>(cfg);
            var path = redirect?.Path;
            if (!path.EndsWith(".yml"))
                path += ".yml";
            return FileUtils.GetFullPath(path);
        }

        public T ReadOptions(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Options file not found: [{path}]");
            Log.Debug("Reading config: [{Path}]", path);
            var cfg = File.ReadAllText(path);
            var opts = _deser.Deserialize<T>(cfg);
            Log.Debug("Config deserialized.");
            PostProcess(opts);
            Log.Debug("Config prepared.");
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
