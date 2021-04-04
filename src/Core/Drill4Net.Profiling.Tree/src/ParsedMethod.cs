namespace Drill4Net.Profiling.Tree
{
    public class ParsedMethod
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string Return { get; set; }
        public string Parameters { get; set; }

        /*************************************************/

        public ParsedMethod(string @namespace, string @return, string name, string parameters)
        {
            Namespace = @namespace;
            Name = name;
            Return = @return;
            Parameters = parameters;
        }
    }
}
