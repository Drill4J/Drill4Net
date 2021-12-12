using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Drill4Net.Agent.Abstract
{
    //https://github.com/Drill4J/test2code-plugin/blob/fc29e9002a493f240cea601ccedfb8a09b1ac584/api/src/commonMain/kotlin/Model.kt#L79

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class TestDetails
    {
        /// <summary>
        /// Testing engine (xUnit, NUnit, SpecFlow + xUnit, etc)
        /// </summary>
        public string engine { get; set; }

        /// <summary>
        /// Contextual "path" - may vary in meaning for different autotest agents.
        /// For ordinal tests it is type full name (?)
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// Name of test's method
        /// </summary>
        public string testName { get; set; }

        /// <summary>
        /// metadata about test for Admin side
        /// </summary>
        [JsonProperty(PropertyName = "params")]
        public Dictionary<string, string> testParams { get; set; } = new();

        /// <summary>
        /// Arbitrary metadata from/for Agent (its specific goals)
        /// </summary>
        public Dictionary<string, string> metadata { get; set; } = new();

        /********************************************************************/

        public override string ToString()
        {
            return $"{engine}: {testName}@{path}";
        }
    }
}
