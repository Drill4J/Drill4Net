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
                _outputHelper.WriteLine("\nCommand:", ConfiguratorAppConstants.COLOR_TEXT);
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
                    _outputHelper.WriteLine($"error -> {ex.Message}", ConfiguratorAppConstants.COLOR_ERROR);
                }
            }
        }

        private bool ProcessCommand(string input)
        {
            input = input.Trim();
            return input switch
            {
                "?" or "help" => _outputHelper.PrintMenu(),
                ConfiguratorAppConstants.COMMAND_SYS => ConfigSystem(),
                ConfiguratorAppConstants.COMMAND_TARGET => ConfigNewTarget(),
                _ => _outputHelper.PrintMenu(),
            };
        }

        internal bool ConfigSystem()
        {
            if (!ConfigAdmin(_rep.Options))
                return false;
            if(!ConfigMiddleware(_rep.Options))
                return false;
            _outputHelper.WriteLine("\nSystem options are retrieved", ConfiguratorAppConstants.COLOR_TEXT);
            return true;
        }

        internal bool ConfigMiddleware(ConfiguratorOptions opts)
        {
            string host;
            var def = opts.MiddlewareHost;
            do
            {
                host = AskQuestion("Kafka host", def);
            }
            while (!CheckStringAnswer(ref host, def, "The Kafka host address cannot be empty"));
            //
            int port;
            def = opts.MiddlewarePort.ToString();
            string portS;
            do
            {
                portS = AskQuestion("Kafka port", def);
            }
            while (!CheckIntegerAnswer(portS, def, "The Kafka port must be from 255 to 65535", 255, 65535, out port));
            //
            var url = $"{host}:{port}";
            _logger.Info($"Kafka url: {url}");

            //
            return true;
        }

        internal bool ConfigAdmin(ConfiguratorOptions opts)
        {
            string host;
            var def = opts.AdminHost;
            do
            {
                host = AskQuestion("Drill service host", def);
            }
            while (!CheckStringAnswer(ref host, def, "The service host address cannot be empty"));
            //
            int port;
            def = opts.AdminPort.ToString();
            string portS;
            do
            {
                portS = AskQuestion("Drill service port", def);
            }
            while (!CheckIntegerAnswer(portS, def, "The service port must be from 255 to 65535", 255, 65535, out port));
            //
            var url = $"{host}:{port}";
            _logger.Info($"Admin url: {url}");

            //
            string plugDir;
            def = opts.PluginDirectory;
            do
            {
                plugDir = AskQuestion("Agent plugin directory", def);
            }
            while (!CheckDirectoryAnswer(ref plugDir, def, true));
            _logger.Info($"Plugin dir: {plugDir}");

            //
            return true;
        }

        internal bool ConfigNewTarget()
        {
            return false;
        }

        private string AskQuestion(string question, string defValue)
        {
            _outputHelper.WriteLine($"\n{question} [{defValue}]: ", ConfiguratorAppConstants.COLOR_QUESTION);
            var answer = Console.ReadLine()?.Trim() ?? defValue;
            _logger.Info($"Question: [{question}]; Default: [{defValue}]; Answer: [{answer}]");
            return answer ?? defValue;
        }

        private bool CheckStringAnswer(ref string answer, string defValue, string mess, bool canBeNull = false)
        {
            var noInput = answer?.Length == 0; //""
            if (noInput)
                answer = defValue;
            if (canBeNull || !string.IsNullOrWhiteSpace(answer))
            {
                if (noInput)
                    _outputHelper.Write(answer, true, ConfiguratorAppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine(mess, ConfiguratorAppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckIntegerAnswer(string answer, string defValue, string mess, int min, int max, out int val)
        {
            var noInput = answer?.Length == 0; //""
            if (noInput)
                answer = defValue;
            if (int.TryParse(answer, out val) && val >= min && val <= max)
            {
                if (noInput)
                    _outputHelper.Write(answer, true, ConfiguratorAppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine(mess, ConfiguratorAppConstants.COLOR_TEXT_WARNING);
            return false;
        }

        private bool CheckDirectoryAnswer(ref string directory, string defValue, bool mustExist = true)
        {
            var noInput = directory?.Length == 0; //""
            if (noInput)
                directory = defValue;
            if (!string.IsNullOrWhiteSpace(directory) && (!mustExist || (mustExist && Directory.Exists(directory))))
            {
                if(noInput)
                    _outputHelper.Write(directory, true, ConfiguratorAppConstants.COLOR_ANSWER);
                return true;
            }
            _outputHelper.WriteLine("Such directory does not exists.", ConfiguratorAppConstants.COLOR_TEXT_WARNING);
            return false;
        }
    }
}
