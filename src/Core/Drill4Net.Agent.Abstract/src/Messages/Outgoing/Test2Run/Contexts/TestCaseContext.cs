namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Context of test case
    /// </summary>
    public class TestCaseContext : BaseTestContext
    {
        /// <summary>
        /// Engine of test framework, maybe + version (moniker, etc)
        /// </summary>
        public string Engine { get; set; }

        /// <summary>
        /// Type of Test Engine (in most cses it is the Hooks class)
        /// </summary>
        public string EngineTypeName { get; set; }

        /// <summary>
        /// Test adapter: xUnit, NUnit, MsTest, etc
        /// </summary>
        public string Adapter { get; set; }

        /// <summary>
        /// Group (trait) of the test
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Fully qualified name of the test (e.g. SortByIsLatestDealVersion)
        /// </summary>
        public string QualifiedName { get; set; }

        /// <summary>
        /// Display name of the test (e.g. "Request with invalid parameters")
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Full "signature" of test case, e.g. for BDD tests:
        /// 'Sort by deal dates(scenarioDescription: "Asc sorting DealCreatedDate", sortField: "DealCreatedDate", sortDirection: "Ascending", versionsReturned: "5,6,4", exampleTags: [])'
        /// </summary>
        public string CaseName { get; set; }

        public long StartTime { get; set; }

        public long FinishTime { get; set; }

        public bool IsFinished { get; set; }

        /// <summary>
        /// Tests's execution result
        /// </summary>
        public string Result { get; set; }

        /********************************************************************/

        public string GetKey()
        {
            var key = CaseName ?? QualifiedName ?? DisplayName;
            //key = System.Web.HttpUtility.UrlEncode(key); //different test cases are different tests
            //key = key.GetHashCode().ToString(); // TEST!!!
            return key;
        }
    }
}
