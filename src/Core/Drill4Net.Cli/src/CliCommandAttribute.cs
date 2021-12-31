namespace Drill4Net.Cli
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CliCommandAttribute : Attribute
    {
        /// <summary>
        /// Command ID: a set of contexts + the name of the command, separated by an underscore.
        /// The parts of the identifier must be sorted alphabetically.
        /// For example: ADD_CONFIG_TARGET: here ADD is context's command, Config and Target 
        /// are the tag contexts for this command.
        /// </summary>
        public string Id { get; set; }

        /*****************************************************************/

        public CliCommandAttribute(params string[] contexts)
        {
            Id = CreateId(contexts);
        }

        /*****************************************************************/

        public static string CreateId(params string[] contexts)
        {
            return CreateId(contexts.ToList());
        }

        public static string CreateId(List<string> contexts)
        {
            var id = "";
            var ordered = contexts.Select(x => x.ToUpper()).OrderBy(a => a);
            if (ordered.Any())
            {
                foreach (var part in ordered)
                    id += part + "_";
                id = id.Substring(0, id.Length - 1);
            }
            return id;
        }
    }
}
