using System;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// Metadata about method's signature
    /// </summary>
    [Serializable]
    public class MethodSignature
    {
        public string Namespace { get; }
        public string Type { get; }
        public string Name { get; }
        public string Return { get; }
        public string Parameters { get; }

        /**************************************************************/
        
        public MethodSignature()
        {
        }

        public MethodSignature(string @namespace, string type, string @return, string name, string parameters)
        {
            Namespace = @namespace;
            Type = type;
            Name = name;
            Return = @return;
            Parameters = parameters;
        }

        /**************************************************************/

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Namespace) ?
                $"{Return} {Name}({Parameters})" :
                $"{Return} {Namespace}.{Type}.{Name}({Parameters})";
        }
    }
}
