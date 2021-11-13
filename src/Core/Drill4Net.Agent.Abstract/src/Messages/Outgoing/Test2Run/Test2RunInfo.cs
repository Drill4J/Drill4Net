using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.Abstract
{
    //https://github.com/Drill4J/test2code-plugin/blob/439aa31ce958383845487f8361c9c3db455e1359/api/src/commonMain/kotlin/Model.kt#L322

    /// <summary>
    /// Info about test case
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class Test2RunInfo
    {
        public string name { get; set; }
        public TestName testName { get; set; }
        public long startedAt { get; set; }
        public long finishedAt { get; set; }
        public string result { get; set; }
        public Test2RunMetadata metadata { get; set; }

        /************************************************************************/

        public Test2RunInfo(string name, TestName testName, long startedAt, string result, Dictionary<string, string> metadata)
        {
            this.name = name;
            this.startedAt = startedAt;
            this.result = result;
            this.testName = testName;
            this.metadata = new Test2RunMetadata()
            {
                data = metadata,
                hash = name,
            };
        }
    }
}
