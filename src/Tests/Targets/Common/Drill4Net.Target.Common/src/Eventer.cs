namespace Drill4Net.Target.Common
{
    public delegate void NotifyHandler(string message);

    public class Eventer
    {
        public event NotifyHandler Notify;

        public void NotifyAbout(string mess)
        {
            Notify?.Invoke(mess);
        }
    }
}
