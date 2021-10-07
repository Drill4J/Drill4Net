using System;
using System.Collections.Generic;
using Drill4Net.Common;

namespace Drill4Net.Injector.Core.src.Helpers
{
    /// <summary>
    /// Helper for working with regex patterns in filters
    /// </summary>
    public static class FilterHelper
    {
        ///<summary>
        /// Check if filter is regex (starts with reegex prefix).
        /// </summary>
        /// <param name="filter">Filter condition</param>
        /// <returns></returns>
        public static bool IsFilterWithRegex(string filter)
        {
            if (filter.StartsWith(CoreConstants.REGEX_FILTER_PPREFIX))
            {
                return true;
            }
            return false;
        }
        ///<summary>
        /// Get regex pattern from filter string.
        /// </summary>
        /// <param name="filter">Filter condition</param>
        /// <returns>Regex pattern string</returns>
        public static string GetRegexPatternForFilter(string filter)
        {
            if (!IsFilterWithRegex(filter))
            {
                throw new ArgumentNullException(nameof(filter), $"Regex filter should start with {CoreConstants.REGEX_FILTER_PPREFIX} prefix."); ;
            }
            return filter.Substring(CoreConstants.REGEX_FILTER_PPREFIX.Length);
        }
        ///<summary>
        /// Check if string matches regex pattern.
        /// </summary>
        /// <param name="s">String for checking</param>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        public static bool IsMatchRegexFilterPattern(string s, string filter)
        {
            if (FilterHelper.IsFilterWithRegex(filter))
            {
                var regexPattern = FilterHelper.GetRegexPatternForFilter(filter);
                return CommonUtils.IsStringMachRegexPattern(s, regexPattern);
            }
            return false;
        }
        ///<summary>
        /// Check if string matches regex patterns.
        /// </summary>
        /// <param name="s">String for checking</param>
        /// <param name="filters">Filters</param>
        /// <returns></returns>
        public static bool IsMatchRegexFilterPattern(string s, List<string> filters)
        {
            foreach (var filter in filters)
            {
                if (FilterHelper.IsMatchRegexFilterPattern(s, filter))
                    return true;
            }
            return false;
        }
    }
}
