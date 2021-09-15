using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Compressor.Benchmarks.Helpers
{
    internal static class DataChecker
    {
        internal static void CounterChecker (ref double indicator, ref double min, ref double max, ref double total)
        {
            if (min > indicator || min == 0)
            {
                min = indicator;
            }
            if (max < indicator)
            {
                max = indicator;
            }
            total = total + indicator;
        }

    }
}
