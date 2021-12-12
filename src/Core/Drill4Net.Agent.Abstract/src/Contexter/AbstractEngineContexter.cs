using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Manager rerieving execution context (name of current testcases) for cpecific engines: xUnit, NUnit, etc
    /// </summary>
    public abstract class AbstractEngineContexter : IEngineContexter
    {
        public string Name { get; }

        protected List<int> _comTypes;

        /******************************************************************************/

        protected AbstractEngineContexter(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Name of plugin cannot be empty");
            Name = name;
            _comTypes = Enum.GetValues(typeof(AgentCommandType)).Cast<int>().ToList();
        }

        /******************************************************************************/

        public abstract string GetContextId();
        public abstract bool RegisterCommand(int command, string data);

        protected TestCaseContext GetTestCaseContext(string str)
        {
            return JsonConvert.DeserializeObject<TestCaseContext>(str);
        }

        public abstract TestEngine GetTestEngine();
    }
}