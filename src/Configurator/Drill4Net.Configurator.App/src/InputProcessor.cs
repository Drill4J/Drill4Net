using System;
using System.Linq;
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
                _outputHelper.WriteMessage("\nCommand:", ConfiguratorAppConstants.COLOR_TEXT);
                var input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (input == "q" || input == "Q")
                    return;
                try
                {
                    ProcessCommand(input);
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteMessage($"error -> {ex.Message}", ConfiguratorAppConstants.COLOR_ERROR);
                }
            }
        }

        private bool ProcessCommand(string input)
        {
            input = input.Trim();
            return input switch
            {
                "?" or "help" => _outputHelper.PrintMenu(),
                "admin" => ConfigAdmin(),
                "new" => ConfigNewTarget(),
                _ => _outputHelper.PrintMenu(),
            };
        }

        internal bool ConfigAdmin()
        {
            string answer;
            do
            {
                answer = AskQuestion("Drill service host", "localhost");
            }
            while (!CheckStringAnswer(answer, "The service host address cannot be empty"));
            int port;
            do
            {
                answer = AskQuestion("Drill service port", "8090");
            }
            while (!CheckIntegerAnswer(answer, "The service port must be from 255 to 65535", 255, 65535, out port));
            //
            var url = $"{answer}:{port}";
            //
            string plugDir;
            do
            {
                plugDir = AskQuestion("Agent plugin directory", _rep.Options.PluginDirectory);
            }
            while (!CheckDirectoryAnswer(plugDir, true));
            //
            return true;
        }

        internal bool ConfigNewTarget()
        {
            return false;
        }

        private string AskQuestion(string question, string defValue)
        {
            _outputHelper.WriteMessage($"{question} [{defValue}]: ", ConfiguratorAppConstants.COLOR_QUESTION);
            var answer = Console.ReadLine()?.Trim() ?? defValue;
            _logger.Info($"Question: [{question}]; Default: [{defValue}]; Answer: [{answer}]");
            return answer ?? defValue;
        }

        private bool CheckStringAnswer(string answer, string mess, bool canBeNull = false)
        {
            if (canBeNull || !string.IsNullOrWhiteSpace(answer))
                return true;
            _outputHelper.WriteMessage(mess, ConfiguratorAppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckIntegerAnswer(string answer, string mess, int min, int max, out int val)
        {
            if (int.TryParse(answer, out val) && val >= min && val <= max)
                return true;
            _outputHelper.WriteMessage(mess, ConfiguratorAppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckDirectoryAnswer(string directory, bool mustExist = true)
        {
            if (!string.IsNullOrWhiteSpace(directory) && (!mustExist || (mustExist && Directory.Exists(directory))))
                return true;
            _outputHelper.WriteMessage("Such directory does not exists.", ConfiguratorAppConstants.COLOR_TEXT_WARNING);
            return false;
        }
    }
}
