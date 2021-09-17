namespace Drill4Net.Compressor.Benchmarks.Helpers
{
    internal static class DataChecker
    {
        /// <summary>
        /// Test for Deflate Compressor
        /// </summary>
        /// <param name="indicator">Indicator</param>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <param name="total"> Cumulative value</param>
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
