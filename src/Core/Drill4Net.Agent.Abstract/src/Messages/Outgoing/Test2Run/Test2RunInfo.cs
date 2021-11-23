using System;

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
        public string id { get; set; }

        // TODO EPMDJ-9150 Replace name with test hash(id)
        public string name { get; set; }
        public TestDetails details { get; set; }
        public long startedAt { get; set; }
        public long finishedAt { get; set; }
        public string result { get; set; }

        /************************************************************************/

        public Test2RunInfo(string name, TestDetails testName, long startedAt, string result)
        {
            this.id = name;
            this.name = name;
            this.startedAt = startedAt;
            this.result = result;
            this.details = testName;
        }

        /************************************************************************/

        public override string ToString()
        {
            return $"{name} -> [{details}] -> Result: {result}";
        }
    }
}
