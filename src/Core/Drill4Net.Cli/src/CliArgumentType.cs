namespace Drill4Net.Cli
{
    public enum CliArgumentType
    {
        /// <summary>
        /// Is it normal parameter name and its value?
        /// </summary>
        NameAndValue,

        /// <summary>
        /// Is it alone parameter's value (not switch because without prefixes "/", "-", "--")?
        /// </summary>
        OnlyValue,

        /// <summary>
        /// Is it CLI switch (set of chars with prefix "-")?
        /// </summary>
        Switch,
    }
}
