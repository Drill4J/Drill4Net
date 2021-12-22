using System;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.BanderLog;

namespace Drill4Net.Configurator.App
{
    internal class InputProcessor
    {
        private readonly ConfiguratorOutputHelper _outputHelper;
        private readonly Logger _logger;

        /**********************************************************************/

        public InputProcessor(ConfiguratorOutputHelper outputHelper)
        {
            _logger = new TypedLogger<InputProcessor>(CoreConstants.SUBSYSTEM_CONFIGURATOR);
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }

        /**********************************************************************/

        public void Start(string[] args)
        {
            //_outputHelper.WriteMessage("Configurator is initializing...", ConfiguratorAppConstants.COLOR_TEXT);
            if (args.Length == 0) //interactive poller
            {
                _logger.Info("Interactive mode");
                _outputHelper.PrintMenu();
                StartInteractive();
            }
            else //automatic processing by arguments
            {
                _logger.Info("Automatic mode");
                ProcessArguments(args);
            }
        }

        internal void ProcessArguments(string[] args)
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
                    break;
                try
                {
                    ProcessCommand(input);
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteMessage($"error -> {ex.Message}", ConfiguratorAppConstants.COLOR_ERROR);
                }
            }

            Console.ReadKey(true);
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
            return false;
        }

        internal bool ConfigNewTarget()
        {
            return false;
        }
    }
}
