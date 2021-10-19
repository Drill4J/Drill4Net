﻿using System.Linq;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Transmitter.SpecFlow
{
    /// <summary>
    /// Helper for SpecFlow part of the point data transmitter
    /// </summary>
    public static class ContextHelper
    {
        //public static string GetFeatureContext(FeatureContext ctx, string asmPath)
        //{
        //    FeatureInfo info = ctx.FeatureInfo;
        //    var testCtx = new TestContext
        //    {
        //        AssemblyPath = asmPath,
        //        Group = GetTestGroup(info),
        //        Tags = info.Tags.ToList(),
        //    };
        //    var data = JsonConvert.SerializeObject(testCtx);
        //    return data;
        //}

        public static string GetScenarioContext(FeatureContext featureCtx, ScenarioContext scenarioCtx, string asmPath)
        {
            var info = scenarioCtx.ScenarioInfo;
            var caseCtx = new TestCaseContext
            {
                AssemblyPath = asmPath,
                Group = GetTestGroup(featureCtx.FeatureInfo),
                QualifiedName = GetQualifiedName(scenarioCtx.ScenarioInfo.Title),
                DisplayName = info.Title,
                CaseName = GetTestCase(scenarioCtx.ScenarioInfo),
                Result = GetTestResult(scenarioCtx),
                Tags = info.Tags.ToList(),
            };
            var data = JsonConvert.SerializeObject(caseCtx);
            return data;
        }

        ////////////////////////////////////////////////////////////////////////

        private static string GetTestResult(ScenarioContext ctx)
        {
            var res = ctx.ScenarioExecutionStatus switch
            {
                ScenarioExecutionStatus.OK => TestResult.PASSED,
                //ScenarioExecutionStatus.StepDefinitionPending => throw new System.NotImplementedException(),
                //ScenarioExecutionStatus.UndefinedStep => throw new System.NotImplementedException(),
                ScenarioExecutionStatus.BindingError => TestResult.ERROR,
                ScenarioExecutionStatus.TestError => TestResult.FAILED,
                ScenarioExecutionStatus.Skipped => TestResult.SKIPPED,
                _ => TestResult.UNKNOWN,
            };
            return res.ToString();
        }

        private static string GetTestGroup(FeatureInfo info)
        {
            return $"{info.FolderPath}/{info.Title}Feature";
        }

        private static string GetTestCase(ScenarioInfo info)
        {
            //Sort by deal dates(scenarioDescription: "Asc sorting DealCreatedDate", sortField: "DealCreatedDate", sortDirection: "Ascending", versionsReturned: "5,6,4", exampleTags: [])
            var title = info.Title;
            var args = info.Arguments;
            var tags = info.Tags;
            var isParams = args.Count > 0 || tags.Length > 0;
            if (isParams)
                title += "(";
            //
            if (args.Count > 0)
            {
                var argsS = string.Empty;
                foreach (System.Collections.DictionaryEntry entry in args)
                {
                    //paramName
                    var key = entry.Key.ToString().Replace(" ", null);
                    char[] a = key.ToCharArray();
                    a[0] = char.ToLower(a[0]);
                    key = new string(a);

                    argsS += $"{key}: \"{entry.Value}\", ";
                }
                title += argsS;
            }
            //
            if (isParams)
                title += "exampleTags: [";
            if (tags.Length > 0)
            {
                foreach (var tag in tags)
                    title += tag + ", ";
                title = title.Substring(0, title.Length-2);
            }
            if (isParams)
                title += "])";
            return title;
        }

        /// <summary>
        /// Converts "Request with invalid parameters" -> "RequestWitInvalidParameters".
        /// In fact, it is just part of full quailifies name (full method name) - short name
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        private static string GetQualifiedName(string displayName)
        {
            if (!displayName.Contains(" "))
                return displayName; //as is
            var ar = displayName.Split(' ');
            for (int i = 0; i < ar.Length; i++)
            {
                string word = ar[i];
                if (string.IsNullOrWhiteSpace(word))
                    continue;
                char[] a = word.ToLower().ToCharArray();
                a[0] = char.ToUpper(a[0]);
                ar[i] = new string(a);
            }
            displayName = string.Join(null, ar).Replace(" ", null);
            return displayName;
        }
    }
}
