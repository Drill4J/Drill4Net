using System;

namespace Drill4Net.Agent.Abstract
{
    //https://github.com/Drill4J/test2code-plugin/blob/fc29e9002a493f240cea601ccedfb8a09b1ac584/api/src/commonMain/kotlin/Model.kt#L79

    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class TestName
    {
        public string engine { get; set; }
        public string className { get; set; }
        public string method { get; set; }
        public string classParams { get; set; } = "";
        public string methodParams { get; set; } = "()";

        /**********************************************************/

        public override string ToString()
        {
            var cl = className;
            if(classParams != null)
                cl += ":" + classParams;
            var mth = method;
            if (methodParams != null)
                mth += ":" + methodParams;
            return $"{engine}: {cl}/{mth}";
        }
    }
}
