using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Drill4Net.Target.NetCore.Tests")]

namespace Drill4Net.Target.Net50
{
    public class InjectTarget
    {
        public async Task RunTests()
        {
            Console.WriteLine($"\n*** {typeof(InjectTarget).FullName} ***\n");

            #region Switch
            Switch_Relational(-5);
            Switch_Relational(5);
            Switch_Relational(10);
            Switch_Relational(100);

            Switch_Logical(-5);
            Switch_Logical(5);
            Switch_Logical(10);
            #endregion
        }

        #region Switch
        //C#9
        internal double Switch_Relational(double sum)
        {
            var newSum = sum switch
            {
                <= 0 => 0,
                < 10 => sum * 1.05,
                < 100 => sum * 1.1,
                _ => sum * 1.15
            };
            Console.WriteLine($"{nameof(Switch_Relational)}: {newSum:F2}");
            return sum;
        }

        //C#9
        internal string Switch_Logical(int a)
        {
            var s = a switch
            {
                <= 0 => "zero",
                > 0 and < 10 => "small",
                _ => "big"
            };
            Console.WriteLine($"{nameof(Switch_Logical)}: {s}");
            return s;
        }
        #endregion

    }
}
