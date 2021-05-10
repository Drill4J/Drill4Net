using System;

namespace Drill4Net.Demo.Target
{
    public class CoverageTarget
    {
        public void IfElse_FullSimple_1(bool cond)
        {
            string type;
            if (cond)
                type = "yes";
            else
                type = "no";

            Console.WriteLine($"{nameof(IfElse_FullSimple_1)}: {type}");
        }

        public void IfElse_FullSimple_2(bool cond)
        {
            string type;
            if (cond)
                type = "yes";
            else
                type = "no";

            Console.WriteLine($"{nameof(IfElse_FullSimple_2)}: {type}");
        }
    }
}
