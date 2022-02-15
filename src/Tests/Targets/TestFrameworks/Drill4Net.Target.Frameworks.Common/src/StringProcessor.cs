namespace Drill4Net.Target.Frameworks.Common
{
    public class StringProcessor
    {
        public string Uppercase(string str)
        {
            return str?.ToUpper(); //ToUpperString(str);
        }

        //internal string ToUpperString(string str)
        //{
        //    return str?.ToUpper();
        //}
    }
}
