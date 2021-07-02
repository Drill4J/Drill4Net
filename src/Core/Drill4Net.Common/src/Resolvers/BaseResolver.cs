namespace Drill4Net.Common
{
    public abstract class BaseResolver
    {
        public string WworkDir { get; }

        protected BaseResolver(string wworkDir = null)
        {
            WworkDir = wworkDir ?? FileUtils.GetEntryDir();
        }
    }
}
