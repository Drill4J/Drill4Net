using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Configurator.App
{
    internal class ConfiguratorOutputHelper
    {
        internal bool PrintMenu()
        {
            const string mess = @"  *** This is help for useful commands:
  *** Enter 'info' for the tree info.
  *** Enter '?' or 'help' to print this menu.
  *** Press q for exit.";
            WriteMessage($"\n{mess}", ConfiguratorAppConstants.COLOR_TEXT_HIGHLITED);
            return true;
        }

        internal void WriteMessage(string mess, ConsoleColor color = ConfiguratorAppConstants.COLOR_DEFAULT)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(mess);
            Console.ForegroundColor = ConfiguratorAppConstants.COLOR_DEFAULT;
        }
    }
}
