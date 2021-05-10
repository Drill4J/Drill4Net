using System;
using System.Diagnostics;

namespace Drill4Net.Demo.Target
{
    /// <summary>
    /// Functions for demo with known coverage part
    /// </summary>
    public class CoverageTarget
    {
        public void IfElse_FullSimple_1(bool cond)
        {
            string type;
            if (cond)
                type = "yes";
            else
                type = "no";

            Debug.WriteLine($"{nameof(IfElse_FullSimple_1)}: {type}");
        }

        public void IfElse_FullSimple_2(bool cond)
        {
            string type;
            if (cond)
                type = "yes";
            else
                type = "no";

            Debug.WriteLine($"{nameof(IfElse_FullSimple_2)}: {type}");
        }
    }
}
