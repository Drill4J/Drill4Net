using System.IO;
using Drill4Net.Common;

namespace Drill4Net.Agent.Standard.Tester
{
    /// <summary>
    /// Get Tester options
    /// </summary>
    internal static class TesterOptionsHelper
    {
        internal static TesterOptions GetOptions()
        {
            var cfgPath = Path.Combine(FileUtils.ExecutingDir, TesterConstants.CONFIG_NAME);
            var deser = new YamlDotNet.Serialization.Deserializer();
            return deser.Deserialize<TesterOptions>(File.ReadAllText(cfgPath));
        }
    }
}
