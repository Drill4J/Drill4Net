using System.Reflection;

namespace Drill4Net.Injector.Plugins.SpecFlow
{
    /// <summary>
    /// It's just for the primier of the fields and methods needed for the injections
    /// </summary>
    #pragma warning disable RCS1102 // Make class static.
    public class DemoTransmitter
    {
        private static MethodInfo _meth;

        /********************************************************/

        public static void DoCommand(int command, string data)
        {
            _meth.Invoke(null, new object[]
            {
                command, data
            });
        }
    }
    #pragma warning restore RCS1102 // Make class static.
}
