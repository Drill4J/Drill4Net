namespace Injector.Engine
{
    /// <summary>
    /// Miscellaneous options for target injection
    /// </summary>
    public class InjectOptions
    {
        public string SourceDirectory { get; set; }

        public string DestinationDirectory { get; set; }

        public bool InjectConstructors { get; set; }
        public bool InjectPrivates { get; set; }
        public bool InjectSetters { get; set; }
        public bool InjectGetters { get; set; }
    }
}
