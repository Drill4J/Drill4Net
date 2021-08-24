using System;
using System.Threading;
using System.Collections.Generic;

namespace Drill4Net.Agent.Messaging
{
    public class Pinger : IDisposable
    {
        private readonly Dictionary<string, string> _state;
        private readonly IPingSender _sender;
        private readonly TimeSpan _period;
        private readonly Timer _timer;

        /***************************************************************/

        public Pinger(string subsystem, string targetSessionUid, IPingSender sender)
        {
            if (string.IsNullOrWhiteSpace(subsystem))
                throw new ArgumentNullException(nameof(subsystem));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _state = new Dictionary<string, string>
            {
                { MessagingConstants.PING_SUBSYSTEM, subsystem },
                { MessagingConstants.PING_TARGET_SESSION, targetSessionUid },
                { MessagingConstants.PING_TIME, GetTime() },
            };

            _period = new TimeSpan(0, 0, 1);
            _timer = new Timer(TimerCallback, null, _period, _period);
        }

        /***************************************************************/

        private void TimerCallback(object state)
        {
            SendPing();
        }

        internal virtual void SendPing()
        {
            _state[MessagingConstants.PING_TIME] = GetTime();
            _sender.SendPing(_state);
        }

        internal string GetTime()
        {
            return DateTime.Now.Ticks.ToString();
        }

        //TODO: full Dispose pattern
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
