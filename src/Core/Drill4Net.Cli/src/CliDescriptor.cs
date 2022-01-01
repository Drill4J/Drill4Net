﻿using System.Text.RegularExpressions;

namespace Drill4Net.Cli
{
    /// <summary>
    /// Arguments class for CLI
    /// </summary>
    public class CliDescriptor
    {
        /// <summary>
        /// ID for this command
        /// </summary>
        public string CommandId { get; private set; } = "";

        /// <summary>
        /// If <see cref="WithCommand"/> if true. List of "contexts" may contains some 
        /// identificators of logical and physical affiliation of some function
        /// (context owners), and the function (command context), but it does not
        /// determine which part of the context is the actual command.
        /// For example, "target config new name=abc.yml -Td": "new" is command in context
        /// of "target" and "config", and "name" is argument for "add" with value "abc.yml",
        /// and "-Td" are two switches: "T" and "d".
        /// </summary>
        public List<string> Contexts { get; } = new();

        /// <summary>
        ///If false, then there is no context and command, and only
        ///application arguments are specified from the first parameter.
        ///The command in this case is implicit: it is the run of the App itself
        /// </summary>
        public bool WithCommand { get; private set; }

        /// <summary>
        /// List of arguments for the command (which located in <see cref="Contexts"/> among others contexts' tags).
        /// </summary>
        public List<CliArgument> Arguments { get; } = new();

        /// <summary>
        /// Get parameter value by its name (switches are not included)
        /// </summary>
        /// <param name="arg"></param>
        /// <returns>Value of parameter</returns>
        public string this[string arg]
        {
            get
            {
                return _argByNames[arg];
            }
        }

        private CliArgument? _switch;
        private readonly Dictionary<string, string> _argByNames = new();

        /***********************************************************************/

        public CliDescriptor(string args, bool withCommand)
        {
            //test TODO: unit tests
            //var a1 = Parse("ci cfg add");
            //var a2 = Parse(@"ci cfg -n ""abc dfe """);
            //var a3 = Parse(@"ci cfg --name =""abc dfe """);
            //var a4 = Parse(@"ci cfg --name = ""abc dfe "" --version = ""1.2.3.4""");
            //var a5 = Parse(@"-n= ""abc dfe "" -Sw");
            //var a6 = Parse(@"cmd -n= ""abc dfe "" -Sw pos1 pos2");
            //var a7 = Parse(@"cmd -n= ""abc dfe "" -Sw -- pos1 pos2");
            //var a8 = Parse(@" -s --degree_parallelism = 4 --cfg_dir = ""d:\Projects\EPM - D4J\"" ");
            //var a9 = Parse(@" -s --cfg_dir = ""d:\Projects\EPM - D4J\"" --degree_parallelism = 4 ");

            //real
            var argsAr = Parse(args);
            Setup(argsAr, withCommand);
        }

        public CliDescriptor(string[] args, bool withCommand)
        {
            Setup(args, withCommand);
        }

        /***********************************************************************/

        /// <summary>
        /// Parse the complete string with some CLI arguments: command, switches, options, etc
        /// </summary>
        /// <param name="args"></param>
        /// <returns>An array of separated arguments, as they are usually passed to the program from the complete command line</returns>
        internal string[] Parse(string args)
        {
            //https://docopt.org/
            var argList = new List<string>();

            var inQuotas = false;
            var block = string.Empty;
            var lastInd = args.Length - 1;
            for (int i = 0; i <= lastInd; i++)
            {
                char ch = args[i];

                if (ch == '"')
                    inQuotas = !inQuotas;
                if (inQuotas)
                {
                    block += ch;
                    continue;
                }
                if (ch == ' ')
                {
                    if(i < lastInd)
                    {
                        var nextChar = args[i + 1];
                        if (nextChar == ' ' || nextChar == '=')
                            continue;
                        if (nextChar == '"')
                        {
                            if(!block.EndsWith("="))
                                block += '=';
                            continue;
                        }
                    }
                    if(i > 0 && args[i - 1] == '=')
                        continue;
                    //
                    block = block.Trim();
                    if(block != string.Empty)
                        argList.Add(block);
                    block = string.Empty;
                }
                else
                {
                    block += ch;
                }
            }
            //
            block = block.Trim();
            if (block != string.Empty)
                argList.Add(block);
            return argList.ToArray();
        }

        private void Setup(string[] args, bool withCommand)
        {
            WithCommand = withCommand;

            Regex splitter = new(@"^-{1,2}|^/|=|:",
                RegexOptions.Compiled);
            Regex remover = new(@"^['""]?(.*?)['""]?$",
                RegexOptions.Compiled);

            string? parameter = null;
            string[] parts;
            int noCommandParameter = 0;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string raw in args)
            {
                var isAloner = !raw.StartsWith("-") && !raw.Contains("=");

                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                parts = splitter.Split(raw, 3);

                switch (parts.Length)
                {
                    // Found a value (for the last parameter 
                    // found (space separator))
                    case 1:
                        if (parameter != null)
                        {
                            if (!_argByNames.ContainsKey(parameter))
                            {
                                parts[0] = remover.Replace(parts[0], "$1");
                                AddParameter(parameter, parts[0], isAloner);
                            }
                            parameter = null;
                        }

                        // it is raw command/contexts
                        if (withCommand)
                            Contexts.Add(raw);
                        else
                            AddParameter($"Parameter{noCommandParameter++}", raw, isAloner);
                        //
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (parameter != null)
                            AddSwitch(parameter);

                        parameter = parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting.
                        // With no value, set it to true.
                        if (parameter != null)
                            AddSwitch(parameter);

                        parameter = parts[1];

                        // Remove possible enclosing characters (",')
                        if (!_argByNames.ContainsKey(parameter))
                        {
                            parts[2] = remover.Replace(parts[2], "$1");
                            AddParameter(parameter, parts[2], isAloner);
                        }

                        parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (parameter != null)
                AddSwitch(parameter);

            Optimize();

            // contexts' ID
            CommandId = CliCommandAttribute.CreateId(Contexts);
        }

        public bool IsSwitchSet(char sw)
        {
            return _switch?.Name?.Contains(sw) == true;
        }

        private void AddParameter(string name, string val, bool isAloner)
        {
            if (_argByNames.ContainsKey(name))
                return;
            _argByNames.Add(name, val);
            Arguments.Add(new CliArgument(name, val, isAloner ? CliArgumentType.OnlyValue : CliArgumentType.NameAndValue));
        }

        /// <summary>
        /// Get the parameter value by its name.
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="isSwitch">Is it CLI switch (one char, e.g. for 'a' in string "-abc" -> is it setted)?</param>
        /// <returns>Value of the parameter. For switches it will be strings "true" or "false"</returns>
        public string? GetParameter(string name, bool isSwitch = false)
        {
            if (isSwitch)
                return IsSwitchSet(name[0]) ? "true" : "false";
            else
                return _argByNames.ContainsKey(name) ? _argByNames[name] : null;
        }

        /// <summary>
        /// Get the alone values (parameters without their names and without prefix "-" or "--")
        /// </summary>
        /// <returns></returns>
        public List<CliArgument> GetAloners() => Arguments.Where(a => a.Type == CliArgumentType.OnlyValue).ToList();

        private void AddSwitch(string name)
        {
            if (_argByNames.ContainsKey(name))
                return;
            const string val = "true";
            _argByNames.Add(name, val);
            Arguments.Add(new CliArgument(name, val, CliArgumentType.Switch));
        }

        /// <summary>
        /// Some optimizations (e.g. switch merging)
        /// </summary>
        private void Optimize()
        {
            //switches from arguments
            var switches = Arguments.Where(a => a.Type == CliArgumentType.Switch).ToList();
            if (switches.Count < 2)
                return;
            var name = "";
            foreach (var sw in switches)
                name += sw.Name;
            _switch = new CliArgument(name, "true", CliArgumentType.Switch);
            Arguments.Add(_switch);
            for (var i = 0; i < switches.Count; i++)
            {
                var sw = switches[i];
                _argByNames.Remove(sw.Name);
                Arguments.Remove(sw);
            }
        }
    }
}
