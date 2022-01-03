using System;
using Drill4Net.Cli;
using Drill4Net.BanderLog;

namespace Drill4Net.Configurator.App
{
    internal class InputProcessor
    {
        public bool IsInteractive { get; private set; }

        private readonly ConfiguratorRepository _rep;
        private readonly CliCommandRepository _cmdRep;
        private readonly ConfiguratorOutputHelper _outputHelper;
        private readonly Logger _logger;

        /**********************************************************************/

        public InputProcessor(ConfiguratorRepository rep, ConfiguratorOutputHelper outputHelper)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
            _logger = new TypedLogger<InputProcessor>(rep.Subsystem);
            _cmdRep = new(rep);
        }

        /**********************************************************************/

        public async Task Start(CliDescriptor cliDesc)
        {
            IsInteractive = cliDesc.Arguments.Count == 0;
            if (IsInteractive) //interactive mode
            {
                _logger.Info("Interactive mode");
                PrintMenu();
                await StartInteractive()
                    .ConfigureAwait(false);
            }
            else //automatic processing by arguments
            {
                _logger.Info("Automatic mode");
                await ProcessByArguments(cliDesc)
                    .ConfigureAwait(false);
            }
        }

        internal void PrintMenu()
        {
            var helpCmd = _cmdRep.GetCommand("?");
            ProcessCommand(helpCmd, null).Wait();
        }

        internal Task ProcessByArguments(CliDescriptor cliDesc)
        {
            var cmd = _cmdRep.GetCommand(cliDesc.CommandId);
            return ProcessCommand(cmd, cliDesc);
        }

        internal async Task StartInteractive()
        {
            while (true)
            {
                _outputHelper.WriteLine("\nCommand:", AppConstants.COLOR_INFO);
                _outputHelper.Write(AppConstants.TERMINAL_SIGN, false, AppConstants.COLOR_DEFAULT);
                var input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (string.Equals(input, ConfiguratorConstants.COMMAND_QUIT, StringComparison.OrdinalIgnoreCase))
                    return;
                try
                {
                    await ProcessCommand(input.Trim())
                       .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"error -> {ex.Message}", AppConstants.COLOR_ERROR);
                }
            }
        }

        private async Task<bool> ProcessCommand(string input)
        {
            try
            {
                //TEST !!!
                //input = "c1 c2 -abc 1 ";
                //input = "c1 -a=1";
                //input = @"cmd -n= ""abc dfe "" -Sw pos0 pos1";
                //input = "copy trg -- cfg cfg3 0.3.0";

                var cmdDesc = new CliDescriptor(input, true);
                var cmd = _cmdRep.GetCommand(cmdDesc.CommandId);
                await ProcessCommand(cmd, cmdDesc)
                        .ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _outputHelper.WriteLine(ex.ToString(), AppConstants.COLOR_ERROR);
                return false;
            }
        }

        internal async Task ProcessCommand(AbstractCliCommand cmd, CliDescriptor desc)
        {
            cmd.Init(desc);
            cmd.MessageDelivered += Command_MessageDelivered;
            await cmd.Process()
                .ConfigureAwait(false);
            cmd.MessageDelivered -= Command_MessageDelivered;
        }

        private void Command_MessageDelivered(string source, string message, CliMessageType messType = CliMessageType.Annotation,
            MessageState state = MessageState.NewLine)
        {
            if (message == "")
                return;
            var color = _outputHelper.ConvertMessageType(messType);
            if (IsInteractive)
            {
                switch (messType)
                {
                    case CliMessageType.EmptyInput:
                        _outputHelper.Write(message + "\n", true, AppConstants.COLOR_INPUT_DEFAULT, true);
                        break;
                    default:
                        switch (state)
                        {
                            case MessageState.CurrentLine:
                                _outputHelper.Write(message, false, color);
                                break;
                            case MessageState.PrevLine:
                                _outputHelper.Write(message, true, color);
                                break;
                            default:
                                _outputHelper.WriteLine(message, color);
                                break;
                        }
                        break;
                }
            }
        }
    }
}
