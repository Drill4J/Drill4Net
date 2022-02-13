namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Engine for the executing the tests (Xunit, NUnit, MsTest, etc)
    /// </summary>
    public class TestEngine
    {
        public string Name { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Tests should be run in sequential, not parallel mode
        /// </summary>
        public bool MustSequential { get; set; }

        /*****************************************************************/

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
