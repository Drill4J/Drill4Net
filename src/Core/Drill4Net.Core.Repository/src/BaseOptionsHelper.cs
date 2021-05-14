using System.IO;
using Serilog;
using YamlDotNet.Serialization;
using Drill4Net.Common;

namespace Drill4Net.Core.Repository
{
    public class BaseOptionsHelper<T> where T : BaseOptions, new()
    {
        public string DefaultCfgPath { get; private set; }

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

        public T GetOptions(string _defaultCfgPath)
        {
            DefaultCfgPath = _defaultCfgPath;
            return ReadOptions(DefaultCfgPath);
        }

        public T ReadOptions(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Options file not found: [{path}]");
            Log.Debug($"Reading config: [{path}]");
            var cfg = File.ReadAllText(path);
            var opts = _deser.Deserialize<T>(cfg);
            PostProcess(opts);
            return opts;
        }

        protected virtual void PostProcess(T opts) { }
    }
}
