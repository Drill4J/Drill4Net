using System;
using Newtonsoft.Json;

namespace Drill4Net.Agent.Abstract
{
    public abstract class AbstractContexter
    {
        public string Name { get; }

        /******************************************************************************/

        protected AbstractContexter(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Name of plugin cannot be empty");
            Name = name;
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