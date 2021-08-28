using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;

namespace Drill4Net.BanderLog.Sinks.File
{
    public class FileSink : AbstractTextSink
    {
        private readonly string _filepath;
        private readonly ChannelsQueue<string> _queue;
        private  StreamWriter _writer;
        private readonly Timer _flushTimer;
        internal static TimeSpan _flushTimeout = new TimeSpan(0, 0, FileSinkConstants.FLUSH_TIMEOUT);
        private readonly object _locker;

        /*****************************************************************************/

        public FileSink(string filepath)
        {
            _locker = new object();
            _filepath = filepath ?? throw new ArgumentNullException(nameof(filepath));
            _queue = new ChannelsQueue<string>(WriteLine);
            _flushTimer = new Timer(TimeFlushing, null, FileSinkConstants.FLUSH_PERIOD / 2, FileSinkConstants.FLUSH_PERIOD);
        }

        /*****************************************************************************/

        private StreamWriter InitializeWriter(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                throw new ArgumentNullException(nameof(filepath));
            return System.IO.File.AppendText(filepath);
        }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var data = FormatData(logLevel, eventId, state, exception, formatter);
            _queue.Enqueue(data);
        }

        /// <summary>
        /// Concrete writing string data to the file
        /// </summary>
        /// <param name="str"></param>
        private void WriteLine(string str)
        {
            if (str == null)
                return;
            lock (_locker)
            {
                try
                {
                    if (_writer == null)
                        _writer = InitializeWriter(_filepath);
                    _writer.WriteLine(str);
                }
                catch { } //TODO: emergency log
            }
        }

        /// <summary>
        /// Raise on timer event for the fluishing of the buferred data to the file
        /// </summary>
        /// <param name="state">Unused state</param>
        private void TimeFlushing(object state)
        {
            EndUpWriter();
        }

        /// <summary>
        /// End up work with the file sink (with the flushing).
        /// </summary>
        public override void Shutdown()
        {
            _queue.Stop();
            _flushTimer.Dispose();
            Flush();
        }

        /// <summary>
        /// Waiting flushing for the all data to the file (with the timeout)
        /// </summary>
        public override void Flush()
        {
            var task = Task.Run(async() =>
            {
                while (_queue.ItemCount > 0)
                    await Task.Delay(10).ConfigureAwait(false);
                EndUpWriter();
            });
            Task.WaitAll(task);
        }

        /// <summary>
        /// Flush data to the file and release the writer
        /// </summary>
        private void EndUpWriter()
        {
            lock (_locker)
            {
                if (_writer == null)
                    return;

                try
                {
                    _writer.Flush();
                }
                catch { } //TODO: emergency log

                _writer.Dispose();
                _writer = null;
            }
        }

        public override string GetKey()
        {
            return _filepath;
        }

        public override string ToString()
        {
            return $"File: {_filepath}";
        }


    }
}
