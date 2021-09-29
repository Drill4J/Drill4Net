using System.Reflection;

namespace Drill4Net.Injection.SpecFlow
{
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
}
