namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Executon context of the test
    /// </summary>
    public class TestContext : BaseTestContext
    {
        /// <summary>
        /// Group (trait) of the test
        /// </summary>
        public string Group { get; set; }
    }
}
