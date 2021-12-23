using System;
using Drill4Net.BanderLog;

namespace Drill4Net.Configurator.App
{
    internal class InputProcessor
    {
        private readonly ConfiguratorRepository _rep;
        private readonly ConfiguratorOutputHelper _outputHelper;
        private readonly Logger _logger;

        /**********************************************************************/

        public InputProcessor(ConfiguratorRepository rep, ConfiguratorOutputHelper outputHelper)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
            _logger = new TypedLogger<InputProcessor>(rep.Subsystem);
        }

        /**********************************************************************/

        public void Start(string[] args)
        {
            //_outputHelper.WriteMessage("Configurator is initializing...", ConfiguratorAppConstants.COLOR_TEXT);
            if (args == null || args.Length == 0) //interactive poller
            {
                _logger.Info("Interactive mode");
                _outputHelper.PrintMenu();
                StartInteractive();
            }
            else //automatic processing by arguments
            {
                _logger.Info("Automatic mode");
                ProcessByArguments(args);
            }
        }

        internal void ProcessByArguments(string[] args)
        {

        }

        internal void StartInteractive()
        {
            while (true)
            {
                _outputHelper.WriteLine("\nCommand:", AppConstants.COLOR_TEXT);
                var input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (string.Equals(input, AppConstants.COMMAND_QUIT, StringComparison.OrdinalIgnoreCase))
                    return;
                try
                {
                    ProcessCommand(input);
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"error -> {ex.Message}", AppConstants.COLOR_ERROR);
                }
            }
        }

        private bool ProcessCommand(string input)
        {
            input = input.Trim();
            return input switch
            {
                "?" or "help" => _outputHelper.PrintMenu(),
                AppConstants.COMMAND_SYS => SystemConfigure(),
                AppConstants.COMMAND_TARGET => TargetConfigure(),
                AppConstants.COMMAND_CI => CIConfigure(),
                _ => _outputHelper.PrintMenu(),
            };
        }

        #region System
        internal bool SystemConfigure()
        {
            SystemConfiguration cfg = new();
            if (!ConfigAdmin(_rep.Options, cfg))
                return false;
            if(!ConfigMiddleware(_rep.Options, cfg))
                return false;

            //TODO: view list of all properties
            //need to save?
            _outputHelper.WriteLine("\nSave the system configuration? [y]:", AppConstants.COLOR_QUESTION);
            var answer = Console.ReadLine()?.Trim();
            var yes = IsYes(answer);
            if (yes)
            {
                _outputHelper.Write("YES", true, AppConstants.COLOR_DEFAULT);
                _rep.SaveSystemConfiguration(cfg);
                _outputHelper.WriteLine($"System options are saved. {AppConstants.MESSAGE_PROPERTIES_EDIT_WARNING}",
                    AppConstants.COLOR_TEXT);
            }
            else
            {
                _outputHelper.Write("NO", true, AppConstants.COLOR_TEXT_WARNING);
            }
            return yes;
        }

        internal bool ConfigAdmin(ConfiguratorOptions opts, SystemConfiguration cfg)
        {
            //Drill host
            string host = null;
            var def = opts.AdminHost;
            do
            {
                if (IsQiut(host))
                    return false;
                host = AskQuestion("Drill service host", def);
            }
            while (!CheckStringAnswer(ref host, def, "The service host address cannot be empty"));
            
            // Drill port
            int port;
            def = opts.AdminPort.ToString();
            string portS = null;
            do
            {
                if (IsQiut(portS))
                    return false;
                portS = AskQuestion("Drill service port", def);
            }
            while (!CheckIntegerAnswer(portS, def, "The service port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.AdminUrl = $"{host}:{port}";
            _logger.Info($"Admin url: {cfg.AdminUrl }");
            
            // agent's plugin dir
            string plugDir = null;
            def = opts.PluginDirectory;
            do
            {
                if (IsQiut(plugDir))
                    return false;
                plugDir = AskQuestion("Agent plugin directory", def);
            }
            while (!CheckDirectoryAnswer(ref plugDir, def, true));
            cfg.AgentPluginDirectory = plugDir;
            _logger.Info($"Plugin dir: {plugDir}");
            //
            return true;
        }

        internal bool ConfigMiddleware(ConfiguratorOptions opts, SystemConfiguration cfg)
        {
            // Kafka host
            string host = null;
            var def = opts.MiddlewareHost;
            do
            {
                if(IsQiut(host))
                    return false;
                host = AskQuestion("Kafka host", def);
            }
            while (!CheckStringAnswer(ref host, def, "The Kafka host address cannot be empty"));
            
            // Kafka port
            int port;
            def = opts.MiddlewarePort.ToString();
            string portS = null;
            do
            {
                if (IsQiut(portS))
                    return false;
                portS = AskQuestion("Kafka port", def);
            }
            while (!CheckIntegerAnswer(portS, def, "The Kafka port must be from 255 to 65535", 255, 65535, out port));
            //
            cfg.MiddlewareUrl = $"{host}:{port}";
            _logger.Info($"Kafka url: {cfg.MiddlewareUrl}");
            return true;
        }
        #endregion
        #region Target
        internal bool TargetConfigure()
        {
            
            return false;
        }
        #endregion
        #region CI
        internal bool CIConfigure()
        {
            _outputHelper.WriteLine("\nSorry, CI operations don't implemented yet", AppConstants.COLOR_TEXT);
            return false;
        }
        #endregion
        #region Common
        private string AskQuestion(string question, string defValue)
        {
            _outputHelper.WriteLine($"\n{question} [{defValue}]: ", AppConstants.COLOR_QUESTION);
            var answer = Console.ReadLine()?.Trim() ?? defValue;
            _logger.Info($"Question: [{question}]; Default: [{defValue}]; Answer: [{answer}]");
            return answer ?? defValue;
        }

        private bool CheckStringAnswer(ref string answer, string defValue, string mess, bool canBeNull = false)
        {
            if (!PrimaryCheckInput(ref answer, defValue, out bool noInput))
                return false;
            if (canBeNull || !string.IsNullOrWhiteSpace(answer))
            {
                if (noInput)
                    _outputHelper.Write(answer, true, AppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine(mess, AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckIntegerAnswer(string answer, string defValue, string mess, int min, int max, out int val)
        {
            val = 0;
            if(!PrimaryCheckInput(ref answer, defValue, out bool noInput))
                return false;
            if (int.TryParse(answer, out val) && val >= min && val <= max)
            {
                if (noInput)
                    _outputHelper.Write(answer, true, AppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine(mess, AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckDirectoryAnswer(ref string directory, string defValue, bool mustExist = true)
        {
            if (!PrimaryCheckInput(ref directory, defValue, out bool noInput))
                return false;
            if (!string.IsNullOrWhiteSpace(directory) && (!mustExist || (mustExist && Directory.Exists(directory))))
            {
                if(noInput)
                    _outputHelper.Write(directory, true, AppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine("Such directory does not exists.", AppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool PrimaryCheckInput(ref string answer, string defValue, out bool noInput)
        {
            noInput = answer?.Length == 0; //""
            if (noInput)
                answer = defValue;
            return !IsQiut(answer);
        }

        private bool IsQiut(string s)
        {
            return string.Equals(s, AppConstants.COMMAND_QUIT, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsYes(string s, bool noInputIsYes = true)
        {
            if (s == "" && noInputIsYes)
                return true;
            return string.Equals(s, AppConstants.COMMAND_YES, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
