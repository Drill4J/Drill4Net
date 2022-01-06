using System.Text.RegularExpressions;

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
            // TODO: unit tests
            //var a1 = Parse("ci cfg add");
            //var a2 = Parse(@"ci cfg -n ""abc dfe """);
            //var a3 = Parse(@"ci cfg --name =""abc dfe """);
            //var a4 = Parse(@"ci cfg --name = ""abc dfe "" --version = ""1.2.3.4""");
            //var a5 = Parse(@"-n= ""abc dfe "" -Sw");
            //var a6 = Parse(@"cmd -n= ""abc dfe "" -Sw pos0 pos1");
            //var a7 = Parse(@"cmd -n= ""abc dfe "" -Sw -- pos0 pos1");
            //var a8 = Parse(@" -s --degree = 4 --cfg_dir = ""d:\EPM - D4J\"" ");
            //var a9 = Parse(@" -s --cfg_dir = ""d:\EPM - D4J\"" --degree = 4 ");
            //var a10 = Parse("c1 c2 --aaa 123 -jhcg");
            //var a11 = Parse(@"c1 c2 --aaa ""123"" -jhcg");
            //var a12 = Parse(@"c1 c2 --aaa= ""123"" -jhcg");
            //var a13 = Parse("c1 -a=1"); //short name parameter
            //var a14 = Parse("c1 c2 -abc=1 "); //improper expression because it is switch, and must be corrected as full name parameter
            //var a15 = Parse("c1 c2 -abc 1 ");
            //var a16 = "copy trg -- cfg cfg3 0.3.0";

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
            var glued = false;
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
                if (ch == '=')
                {
                    glued = true;
                    //input is not short name parameter, so contains improper expression (-aaa=1), and must be corrected as full name parameter
                    if (block.Length > 2 && !block.StartsWith("--"))
                        block = "-" + block;
                }
                if (ch == ' ')
                {
                    if (block != "--")
                    {
                        if (i < lastInd)
                        {
                            var nextChar = args[i + 1];
                            if (nextChar == ' ' || nextChar == '=')
                                continue;
                            if (!glued && (nextChar == '"' || block.StartsWith("--")))
                            {
                                if (!block.EndsWith("="))
                                {
                                    block += '=';
                                    glued = true;
                                }
                                continue;
                            }
                        }
                        if (i > 0 && args[i - 1] == '=')
                            continue;
                    }
                    //
                    block = block.Trim();
                    if(block != string.Empty)
                        argList.Add(block);
                    block = string.Empty;
                    glued = false;
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
            int posParamInd = -1;
            bool wasOptions = false;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string raw in args)
            {
                if (raw.StartsWith("-"))
                    wasOptions = true;
                if (raw == "--")
                    continue;
                var isAloner = !raw.StartsWith("-") && !raw.Contains("=");

                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                parts = splitter.Split(raw, 3);
                if (parts.Length == 2)
                {
                    // it is Windows path in quotes
                    if (parts[0].StartsWith("\"") && !parts[0].EndsWith("\"") &&
                        !parts[1].StartsWith("\"") && parts[1].EndsWith("\""))
                    {
                        parts = new string[] { raw };
                    }
                }

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
                                AddParameter(parameter, parts[0]);
                            }
                            parameter = null;
                        }

                        // it is raw command/contexts
                        if (!wasOptions && withCommand)
                        {
                            Contexts.Add(raw);
                        }
                        else //positional parameter
                        {
                            posParamInd++;
                            AddParameter($"Parameter{posParamInd}", raw, posParamInd);
                        }
                        //
                        break;

                    // Found just a parameter
                    case 2:
                        var isSwitch = wasOptions && !raw.StartsWith("--");
                        if (isSwitch)
                        {
                            AddSwitch(parts[1]);
                            parameter = null;
                        }
                        else
                        {
                            // The last parameter is still waiting. 
                            // With no value, set it to true.
                            if (parameter != null)
                                AddSwitch(parameter);
                            parameter = parts[1];
                        }
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
                            AddParameter(parameter, parts[2]);
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

        private void AddParameter(string name, string val, int posParamInd = -1)
        {
            if (_argByNames.ContainsKey(name))
                return;
            _argByNames.Add(name, val);
            //
            CliArgument arg = posParamInd == -1 ?
                new(name, val, CliArgumentType.NameAndValue) :
                new(name, val, posParamInd);
            Arguments.Add(arg);
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
        public List<CliArgument> GetPositionals() => Arguments.Where(a => a.Type == CliArgumentType.Positional).ToList();

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
            {
                if(switches.Count > 0)
                    _switch = switches[0];
                return;
            }
            //
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
