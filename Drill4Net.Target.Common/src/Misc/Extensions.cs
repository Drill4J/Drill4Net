namespace Drill4Net.Target.Common
{
    public static class Extensions
    {
        public static string ToWord(this bool cond)
        {
            return cond ? "YES" : "NO";
        }
    }
}
