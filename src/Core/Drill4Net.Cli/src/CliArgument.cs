namespace Drill4Net.Cli
{
    /// <summary>
    /// Parsed parameter from CLI (command line interface)
    /// </summary>
    public class CliArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Poisition { get; set; }

        /// <summary>
        /// Is it CLI switch (set of chars with prefix "-")?
        /// </summary>
        public CliArgumentType Type { get; set; }

        /*********************************************************/

        public CliArgument(string name, string value, int position)
        {
            Name = name;
            Value = value;
            Type = CliArgumentType.Positional;
            Poisition = position;
        }

        public CliArgument(string name, string value, CliArgumentType type)
        {
            Name = name;
            Value = value;
            Type = type;
            Poisition = -1;
        }

        /*********************************************************/

        public override string ToString()
        {
            return Type switch
            {
                CliArgumentType.Positional => $"{Poisition}:{Value}",
                CliArgumentType.Switch => $"-{Name}",
                _ => $"{Name}={Value}",
            };
        }
    }
}
