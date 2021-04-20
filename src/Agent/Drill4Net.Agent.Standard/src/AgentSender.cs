using System.Collections.Generic;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Standard
{
    //https://kb.epam.com/pages/viewpage.action?pageId=881283184
    
    public class AgentSender
    {
        //also SCOPE_INITIALIZED ?? or it event from plugin?
        
        public void SendInitInfoMessage()
        {
            
        }
        
        public void SendInitializedMessage()
        {
            
        }

        public void SendStartClassesTransferMessage()
        {
            
        }

        public void SendClassesDataMessage(IEnumerable<AstEntity> entities)
        {
            foreach (var entity in entities)
            {
                
            }
        }
        
        public void SendFinishClassesTransferMessage()
        {
            
        }
        
        //???
        public void SendSessionFinishedMessage()
        {
            
        }
    }
}