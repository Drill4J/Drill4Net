using System.Collections.Generic;

namespace Drill4Net.Target.Common
{
    public class StringEnumerable
    {
        private readonly string[] _data = new string[] { "", "a", "b", "", "c", null };

        /***************************************************************/

        public IEnumerator<string> GetEnumerator()
        {
            return new NotEmptyStringEnumerator(_data);
        }
    }
}
