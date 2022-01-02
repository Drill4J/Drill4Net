﻿namespace Drill4Net.Cli
{
    public enum CliArgumentType
    {
        /// <summary>
        /// Is it normal parameter name and its value (option)?
        /// </summary>
        NameAndValue,

        /// <summary>
        /// Is it positional (alone) parameter's value (not switch because it has no prefixes "/", "-", "--")?
        /// </summary>
        OnlyValue,

        /// <summary>
        /// Is it CLI switch (set of chars with prefix "-")?
        /// </summary>
        Switch,
    }
}
