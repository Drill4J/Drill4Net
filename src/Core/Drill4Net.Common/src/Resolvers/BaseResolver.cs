namespace Drill4Net.Common
{
    public abstract class BaseResolver
    {
        public string WorkDir { get; }

        /**************************************************************/

        protected BaseResolver(string wworkDir = null)
        {
            WorkDir = wworkDir ?? FileUtils.GetEntryDir();
        }
    }
}
