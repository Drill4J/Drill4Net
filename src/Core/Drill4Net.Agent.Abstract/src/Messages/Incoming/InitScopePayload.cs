using System;

namespace Drill4Net.Agent.Abstract.Transfer
{
    [Serializable]
    public record InitScopePayload
    {
        public string Id { get; set; }
        public string  Name { get; set; }
        public string  PrevId { get; set; }
        
        /********************************************************************/

        public InitScopePayload(string id, string name, string prevId)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PrevId = prevId ?? throw new ArgumentNullException(nameof(prevId));
        }
    }
}