using System;
using System.Collections.Generic;

namespace Drill4Net.Common
{
    /// <summary>
    /// Common method's extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Distinct the elements of the sequence by the key selector, for example, by property of the object
        /// </summary>
        /// <typeparam name="TSource">Type of source sequence</typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Key selector, for example, property of the object</param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
