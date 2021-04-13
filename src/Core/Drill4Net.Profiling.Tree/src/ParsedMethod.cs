namespace Drill4Net.Profiling.Tree
{
    public class ParsedMethod
    {
        public string Namespace { get; }
        public string Name { get; }
        public string Return { get; }
        public string Parameters { get; }

        /*************************************************/
        
        public ParsedMethod()
        {
        }

        public ParsedMethod(string @namespace, string @return, string name, string parameters)
        {
            Namespace = @namespace;
            Name = name;
            Return = @return;
            Parameters = parameters;
        }
    }
}
