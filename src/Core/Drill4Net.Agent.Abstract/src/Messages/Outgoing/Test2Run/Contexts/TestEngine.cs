namespace Drill4Net.Agent.Abstract
{
    public class TestEngine
    {
        public string Name { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Tests should be run in sequential, not parallel mode
        /// </summary>
        public bool MustSequential { get; set; }
    }
}
