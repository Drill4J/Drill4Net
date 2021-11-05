using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractContexter
    {
        public string Name { get; }

        protected List<int> _comTypes;

        /******************************************************************************/

        protected AbstractContexter(string name)
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
    }
}