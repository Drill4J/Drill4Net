using System;
using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    [Serializable]
    public class InitInfo : OutgoingMessage
    {
        public string message { get; set; }

        public int classesCount { get; set; }

        public bool init { get; set; }

        /*****************************************************************/

        public InitInfo() : base(AgentConstants.MESSAGE_OUT_INIT) { }

        public InitInfo(int classesCount) : this()
        {
            this.classesCount = classesCount;
            message = "Init";
            init = true; //yes, it is very strange))
        }
    }
}
