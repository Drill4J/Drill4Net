namespace Drill4Net.Common
{
    /// <summary>
    /// Parsed parameter from CLI (command line interface)
    /// </summary>
    public class CliArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// Is it CLI switch (set of chars with prefix "-")?
        /// </summary>
        public CliArgumentType Type { get; set; }

        /*********************************************************/

        public CliArgument(string name, string value, CliArgumentType type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        /*********************************************************/

        public override string ToString()
        {
            return Type switch
            {
                CliArgumentType.OnlyValue => Value,
                CliArgumentType.Switch => $"-{Name}",
                _ => $"{Name}={Value}",
            };
        }
    }
}
