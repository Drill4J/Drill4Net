using System.IO;

namespace Target.Common
{
    public class Finalizer
    {
        public int Prop { get; set; }

        private readonly MemoryStream _ms;

        /*************************************************/

        public Finalizer(int prop)
        {
            Prop = prop;
            _ms = new MemoryStream(new byte[1000]);
        }

        ~Finalizer()
        {
            if (Prop % 2 == 0)
                Prop = 13;
            else
                Prop = 100;

            _ms.Dispose();
        }
    }
}
