using System.IO;

namespace Drill4Net.Target.Common
{
    public class Finalizer
    {
        private readonly MemoryStream _ms;

        /***************************************************/

        public Finalizer(int len)
        {
            _ms = new MemoryStream(new byte[len]);
        }

        ~Finalizer()
        {
            var s = _ms.Length % 2 == 0 ? "if" : "else";
            System.Console.WriteLine($"~Finalizer({s})");

            _ms?.Dispose();
        }
    }
}
