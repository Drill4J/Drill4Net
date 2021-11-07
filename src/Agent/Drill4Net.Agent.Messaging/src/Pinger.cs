using System;
using System.Threading;
using System.Collections.Specialized;

namespace Drill4Net.Agent.Messaging
{
    public class Pinger : IDisposable
    {
        private readonly IMessagerRepository _rep;
        private readonly StringDictionary _state;
        private readonly IPingSender _sender;
        private readonly TimeSpan _period;
        private readonly Timer _timer;
        private bool _isSending;

        /***************************************************************/

        public Pinger(IMessagerRepository rep, IPingSender sender)
        {
            _rep = rep ?? throw new ArgumentNullException(nameof(rep));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _state = new StringDictionary
            {
                { MessagingConstants.PING_SUBSYSTEM, rep.Subsystem },
                { MessagingConstants.PING_TARGET_NAME, rep.TargetName },
                { MessagingConstants.PING_TARGET_SESSION, rep.TargetSession.ToString() },
                { MessagingConstants.PING_TIME, GetTime() },
                { MessagingConstants.PING_MEMORY, GetMemory() },
            };

            _period = new TimeSpan(0, 0, 1);
            _timer = new Timer(TimerCallback, null, _period, _period);
        }

        /***************************************************************/

        private void TimerCallback(object state)
        {
            if (_isSending)
                return;
            _isSending = true;

            try
            {
                SendPing();
            }
            catch
            {
                throw;
            }
            finally
            {
                _isSending = false;
            }
        }

        internal virtual void SendPing()
        {
            _state[MessagingConstants.PING_TIME] = GetTime();
            _state[MessagingConstants.PING_MEMORY] = GetMemory();
            //
            _sender.SendPing(_state); //TODO: by JSON/YAML?
        }

        internal virtual string GetTime()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }

        private string GetMemory()
        {
            return Environment.WorkingSet.ToString();
        }

        //TODO: full Dispose pattern
        public void Dispose()
        {
            _timer?.Dispose();
            _sender?.Dispose();
        }
    }
}
