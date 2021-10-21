using System;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Options for the injecting info about specific runtime plugins
    /// </summary>
    [Serializable]
    public class PluginOptions
    {
        public string Path { get; set; }
    }
}
