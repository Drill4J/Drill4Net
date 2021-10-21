using System;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Context of test case
    /// </summary>
    public class TestCaseContext : BaseTestContext
    {
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
        /// Full "signture" of test case, e.g. for BDD tests:
        /// 'Sort by deal dates(scenarioDescription: "Asc sorting DealCreatedDate", sortField: "DealCreatedDate", sortDirection: "Ascending", versionsReturned: "5,6,4", exampleTags: [])'
        /// </summary>
        public string CaseName { get; set; }

        public long StartTime { get; set; }

        public long FinishTime { get; set; }

        /// <summary>
        /// Tests's execution result
        /// </summary>
        public TestResult Result { get; set; }
    }
}
