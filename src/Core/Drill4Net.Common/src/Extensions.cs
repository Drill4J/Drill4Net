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

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int cnt)
        {
            var res = new List<IEnumerable<T>>();
            if (cnt < 2)
            {
                res.Add(source);
                return res;
            }
            var en = source.GetEnumerator();
            while(true)
            {
                List<T> group = new();
                res.Add(group);
                bool needFinish = false;
                for (var i = 0; i < cnt; i++)
                {
                    if (!en.MoveNext())
                    {
                        needFinish = true;
                        break;
                    }
                    var el = en.Current;
                    group.Add(el);
                }
                if (needFinish)
                    break;
            }
            return res;
        }
    }
}
