using System.IO;

namespace Drill4Net.Target.Common
{
    public class Finalizer
    {
        private readonly MemoryStream _ms;

        /***************************************************/

        public Finalizer(int len)
        {
            #pragma warning disable DF0020 // Marks undisposed objects assinged to a field, originated in an object creation.
            _ms = new MemoryStream(new byte[len]);
            #pragma warning restore DF0020 // Marks undisposed objects assinged to a field, originated in an object creation.
        }

        ~Finalizer()
        {
            var s = _ms.Length % 2 == 0 ? "even" : "odd";
            System.Console.WriteLine($"~Finalizer({s})");

            _ms?.Dispose();
        }
    }
}
