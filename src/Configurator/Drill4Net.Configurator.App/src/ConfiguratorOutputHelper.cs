namespace Drill4Net.Configurator.App
{
    internal class ConfiguratorOutputHelper
    {
        internal bool PrintMenu()
        {
            const string mess = $@"  === Please, type:
  >>> '?' or 'help' to print this menu.
  --- Configurations:
  >>> '{AppConstants.COMMAND_SYS}' for the system settings.
  >>> '{AppConstants.COMMAND_TARGET}' for target's injection.
  >>> '{AppConstants.COMMAND_RUNNER}' for the Test Runner.
  >>> '{AppConstants.COMMAND_CI}' for CI settings.
  --- Actions:
  >>> '{AppConstants.COMMAND_START}' to start full cycle (target injection + tests' running).
  >>> 'q' to exit.";
            WriteLine($"\n{mess}", AppConstants.COLOR_TEXT_HIGHLITED);
            return true;
        }

        internal void WriteLine(string mess, ConsoleColor color = AppConstants.COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = AppConstants.COLOR_DEFAULT;
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
