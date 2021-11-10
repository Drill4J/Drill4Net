using System;

namespace Drill4Net.Agent.Abstract
{
    [Serializable]
    public record class ActiveScopeChangePayload
    {
        public string scopeName { get; set; }
        public bool savePrevScope { get; set; }
        public bool prevScopeEnabled { get; set; }
        public bool forceFinish { get; set; }

        /*********************************************************/

        public override string ToString()
        {
            var name = string.IsNullOrWhiteSpace(scopeName) ? "no name" : scopeName;
            return $"[{name}];forceFinish={forceFinish};savePrevScope={savePrevScope};prevScopeEnabled={prevScopeEnabled}";
        }
    }
}
