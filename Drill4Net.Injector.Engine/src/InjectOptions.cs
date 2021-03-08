namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Miscellaneous options for target injection
    /// </summary>
    public class InjectOptions
    {
        public string ProfilerDirectory { get; set; }
        public string ProfilerAssemblyName { get; set; }
        public string ProfilerNamespace { get; set; }
        public string ProfilerClass { get; set; }
        public string ProfilerMethod { get; set; }

        public string ProxyClass { get; set; }
        public string ProxyMethod { get; set; }

        public string SourceDirectory { get; set; }

        public string DestinationDirectory { get; set; }

        public bool InjectConstructors { get; set; }
        public bool InjectPrivates { get; set; }
        public bool InjectSetters { get; set; }
        public bool InjectGetters { get; set; }
    }
}
