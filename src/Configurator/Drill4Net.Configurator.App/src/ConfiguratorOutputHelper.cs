using Drill4Net.Cli;

namespace Drill4Net.Configurator.App
{
    internal class ConfiguratorOutputHelper
    {
        internal void WriteLine(string mess, CliMessageType messType)
        {
            WriteLine(mess, ConvertMessageType(messType));
        }

        internal ConsoleColor ConvertMessageType(CliMessageType messType)
        {
            return messType switch
            {
                CliMessageType.Input_Default => AppConstants.COLOR_INPUT,
                CliMessageType.Info => AppConstants.COLOR_INFO,
                CliMessageType.Message => AppConstants.COLOR_MESSAGE,
                CliMessageType.Help => AppConstants.COLOR_INPUT,
                CliMessageType.Question => AppConstants.COLOR_QUESTION,
                CliMessageType.Warning => AppConstants.COLOR_WARNING,
                CliMessageType.Error => AppConstants.COLOR_ERROR,
                _ => AppConstants.COLOR_DEFAULT,
            };
        }

        internal void WriteLine(string mess, ConsoleColor color = AppConstants.COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = AppConstants.COLOR_DEFAULT;
        }

        internal void Write(string mess, bool prevLine = false, CliMessageType messType = CliMessageType.Info, bool eraseInvitation = false)
        {
            Write(mess, prevLine, ConvertMessageType(messType), eraseInvitation);
        }

        internal void Write(string mess, bool prevLine = false, ConsoleColor color = AppConstants.COLOR_DEFAULT, bool eraseInvitation = false)
        {
            Console.ForegroundColor = color;
            if (prevLine)
            {
                Console.SetCursorPosition(Console.GetCursorPosition().Left, Console.CursorTop - 1);

                //erase the invitation
                if (eraseInvitation)
                {
                    (var left, var top) = Console.GetCursorPosition();
                    Console.Write(new string(' ', AppConstants.TERMINAL_SIGN.Length));
                    Console.SetCursorPosition(left, top);
                }
            }
            Console.Write(mess);
            if (prevLine)
                Console.SetCursorPosition(0, Console.CursorTop + 1);
            Console.ForegroundColor = AppConstants.COLOR_DEFAULT;
        }
    }
}
