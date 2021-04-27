using Drill4Net.Agent.Abstract.Transfer;

namespace Drill4Net.Agent.Abstract
{
    public class InitInfo : AbstractMessage
    {
        public string Message { get; set; }
        public int ClassesCount { get; set; }
        public bool Init { get; set; }

        /*****************************************************************/

        public InitInfo() : base(AgentConstants.MESSAGE_OUT_INIT) { }

        public InitInfo(int classesCount) : this()
        {
            ClassesCount = classesCount;
            Message = "Init";
            Init = true; //yes, it is very strange))
        }
    }
}
