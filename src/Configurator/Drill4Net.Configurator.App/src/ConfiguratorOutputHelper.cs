namespace Drill4Net.Configurator.App
{
    internal class ConfiguratorOutputHelper
    {
        internal bool PrintMenu()
        {
            const string mess = $@"  *** This is help for useful commands:
  *** Enter '?' or 'help' to print this menu.
  *** Enter '{ConfiguratorAppConstants.COMMAND_SYS}' for config connection to Drill service.
  *** Enter '{ConfiguratorAppConstants.COMMAND_TARGET}' to operate target's injection.
  *** Enter '{ConfiguratorAppConstants.COMMAND_CI}' for some CI operations.
  *** Press q for exit.";
            WriteLine($"\n{mess}", ConfiguratorAppConstants.COLOR_TEXT_HIGHLITED);
            return true;
        }

        internal void WriteLine(string mess, ConsoleColor color = ConfiguratorAppConstants.COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = ConfiguratorAppConstants.COLOR_DEFAULT;
        }

        internal void Write(string mess, bool prevLine = false, ConsoleColor color = ConfiguratorAppConstants.COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            if(prevLine)
                Console.SetCursorPosition(Console.GetCursorPosition().Left, Console.CursorTop - 1);
            Console.Write(mess);
            if (prevLine)
                Console.SetCursorPosition(0, Console.CursorTop + 1);
            Console.ForegroundColor = ConfiguratorAppConstants.COLOR_DEFAULT;
        }
    }
}
