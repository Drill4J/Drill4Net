using System;
using System.Collections.Generic;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Standard
{
    public class StandardAgent
    {
        private readonly IEnumerable<AstEntity> _entities;
        
        /*****************************************************************/
        
        public StandardAgent(IEnumerable<AstEntity> entities)
        {
            _entities = entities ?? throw new ArgumentNullException(nameof(entities));
        }
        
        /*****************************************************************/
    }
}