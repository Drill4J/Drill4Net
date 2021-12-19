using System.Linq;
using System.Diagnostics;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Drill4Net.Common;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Plugins.SpecFlow
{
    /// <summary>
    /// SpecFlow helper for retrieving tests' workflow from the specific contexts
    /// </summary>
    public class SpecFlowGeneratorContexter : IGeneratorContexter
    {
        private static readonly SpecFlowGeneratorContexter _singleton = new();
        private readonly TestGenerator _generator;
        private static readonly ConcurrentDictionary<string, long> _testCaseStartTimes = new();

        /**********************************************************************************************/

        public SpecFlowGeneratorContexter()
        {
            _generator = new TestGenerator
            {
                Name = "SpecFlow",
                Version = FileUtils.GetProductVersion(typeof(FeatureContext)),
            };
        }

        /**********************************************************************************************/

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

        /// <summary>
        /// Get the data about feature and scenario contexts during the test case executing
        /// </summary>
        /// <param name="featureCtx"></param>
        /// <param name="scenarioCtx"></param>
        /// <param name="asmPath"></param>
        /// <returns></returns>
        public static string GetScenarioContext(FeatureContext featureCtx, ScenarioContext scenarioCtx, string asmPath)
        {
            //[CallerMemberName] is not convient here
            var stackTrace = new StackTrace(1, false);
            var type = stackTrace.GetFrame(3).GetMethod().DeclaringType;
            //
            var info = scenarioCtx.ScenarioInfo;
            _singleton._generator.TypeName = type.FullName;
            var caseCtx = new TestCaseContext
            {
                AssemblyPath = asmPath,
                Generator = _singleton._generator,
                //TODO: get Engine from plugins (through TransmitterRepository tranferred to SpecFlowTestContexter's .ctor -
                //this static class must be fully non-static with creating reference in ProfilerProxy)
                //Engine =
                Group = GetTestGroup(featureCtx.FeatureInfo),
                QualifiedName = TestContextHelper.GetQualifiedName(scenarioCtx.ScenarioInfo.Title),
                DisplayName = info.Title,
                CaseName = GetTestCase(scenarioCtx.ScenarioInfo),
                Tags = info.Tags.ToList(),
            };

            //need to get the start time of the test case
            //if value is exist, this call is the finishing of the test case
            var key = caseCtx.GetKey();
            var isFinished = _testCaseStartTimes.TryGetValue(key, out var startTime);
            if (isFinished)
            {
                caseCtx.IsFinished = true;
                caseCtx.StartTime = startTime;
                caseCtx.FinishTime = CommonUtils.GetCurrentUnixTimeMs();
                caseCtx.Result = GetTestResult(scenarioCtx);

                //need to remove due to potential repeat the test
                _testCaseStartTimes.TryRemove(key, out startTime);
            }
            else //starting
            {
                caseCtx.Result = nameof(TestResult.UNKNOWN);
                caseCtx.StartTime = CommonUtils.GetCurrentUnixTimeMs();
                _testCaseStartTimes.TryAdd(key, caseCtx.StartTime);
            }
            return JsonConvert.SerializeObject(caseCtx);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        public TestGenerator GetTestGenerator()
        {
            return _generator;
        }
    }
}
