namespace Drill4Net.Configurator
{
    public class BatchInjectionOptions
    {
        /// <summary>
        /// Directory with the injector configs to be used (all in the folder)
        /// </summary>
        public string? ConfigDir { get; set; }

        public int? DegreeOfParallelism { get; set; }
    }
}