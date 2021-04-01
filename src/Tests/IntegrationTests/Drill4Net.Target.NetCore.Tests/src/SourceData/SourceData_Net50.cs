using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Drill4Net.Target.NetCore.Tests.SourceDataCore;

namespace Drill4Net.Target.NetCore.Tests
{
    internal class SourceData_Net50
    {
        private static readonly Net50.InjectTarget _target50;

        /***************************************************************/

        static SourceData_Net50()
        {
            _target50 = new Net50.InjectTarget();
        }

        /***************************************************************/

        private static IEnumerable<TestCaseData> GetSimple(object target)
        {
            #region Swith_C#9
            yield return GetCase(_target50, GetInfo(_target50.Switch_Relational), new object[] { -5 }, new List<string> { "Else_8", "If_21" });
            yield return GetCase(_target50, GetInfo(_target50.Switch_Relational), new object[] { 5 }, new List<string> { "Else_8", "If_25" });
            yield return GetCase(_target50, GetInfo(_target50.Switch_Relational), new object[] { 10 }, new List<string> { "If_15", "If_31" });
            yield return GetCase(_target50, GetInfo(_target50.Switch_Relational), new object[] { 100 }, new List<string> { "If_15", "If_39" });

            yield return GetCase(_target50, GetInfo(_target50.Switch_Logical), new object[] { -5 }, new List<string> { "If_15" });
            yield return GetCase(_target50, GetInfo(_target50.Switch_Logical), new object[] { 5 }, new List<string> { "Else_8", "If_20" });
            yield return GetCase(_target50, GetInfo(_target50.Switch_Logical), new object[] { 10 }, new List<string> { "Else_8", "If_24" });
            #endregion
        }
    }
}
