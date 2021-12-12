namespace Drill4Net.Agent.Abstract
{
    public static class TestContextHelper
    {
        /// <summary>
        /// Converts "Request with invalid parameters(...)" -> "RequestWitInvalidParameters".
        /// In fact, it is just part of full quailifies name (full method name) - short name
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static string GetQualifiedName(string displayName)
        {
            if (!displayName.Contains(" "))
                return displayName; //as is
            var ar = displayName.Split(' ');
            for (int i = 0; i < ar.Length; i++)
            {
                string word = ar[i];
                if (string.IsNullOrWhiteSpace(word))
                    continue;
                char[] a = word.ToLower().ToCharArray();
                a[0] = char.ToUpper(a[0]);
                ar[i] = new string(a);
            }
            //TODO: regex (check also another bad symbols)
            displayName = string.Join(null, ar)
                .Replace(" ", null)
                .Replace(":", null)
                .Replace("-", "_");
            return displayName;
        }
    }
}
