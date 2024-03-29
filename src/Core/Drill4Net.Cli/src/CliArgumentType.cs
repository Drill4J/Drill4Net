﻿namespace Drill4Net.Cli
{
    public enum CliArgumentType
    {
        /// <summary>
        /// Is it normal parameter with name and value (option)?
        /// A single parameter (--abc) belongs to this type and has implicit boolean type.
        /// </summary>
        NameAndValue,

        /// <summary>
        /// Is it positional (alone) parameter's value (not switch because it has no prefixes "/", "-", "--")?
        /// </summary>
        Positional,

        /// <summary>
        /// Is it CLI switch (set of chars with prefix "-")?
        /// </summary>
        Switch,
    }
}
