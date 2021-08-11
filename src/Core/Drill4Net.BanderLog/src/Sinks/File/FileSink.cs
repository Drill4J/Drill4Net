using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Drill4Net.Common;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks.File
{
    public class FileSink : AbstractTextSink
    {
        private readonly string _filepath;
        private readonly ChannelsQueue<string> _queue;
        private  StreamWriter _writer;
        private DateTime? _lastLogTime;
        private int _linesToFlushCounter;

        /*****************************************************************************/

        internal FileSink(string filepath)
        {
            _filepath = filepath;
        #pragma warning disable DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
            _writer=InitializeWriter(_filepath);
            //Action<string> action = (string str) => File.AppendAllLines(_filepath, new string[] { str }); //opens & closes file each time - for IHS BDD 18:08 min
            _queue = new ChannelsQueue<string>(WriteLineToLogAsync);
        #pragma warning restore DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
        }

        /*****************************************************************************/
        private void WriteLineToLogAsync(string str)
        {
            if (_writer == null)
                _writer = InitializeWriter(_filepath);

                _writer.WriteLine(str);
                _linesToFlushCounter=Interlocked.Increment(ref _linesToFlushCounter);
            
            if ((_linesToFlushCounter >= FileSinkConstants.LINES_TO_FLUSH_COUNTER) || (_lastLogTime != null && (DateTime.Now - _lastLogTime)?.TotalSeconds >= FileSinkConstants.MAX_TIME_GAP_FOR_FLUSH))
            {
                System.Console.WriteLine(_linesToFlushCounter + "   " + (DateTime.Now - _lastLogTime)?.TotalSeconds);
                _writer.Flush();
                _writer.Close();
                _writer = null;
                _queue.AddItemsCounter(-_linesToFlushCounter);
                _linesToFlushCounter = 0;
            }
            _lastLogTime = DateTime.Now;
        }

        private StreamWriter InitializeWriter( string filepath)
        {
            var writer = System.IO.File.AppendText(filepath); //writes to memory and flushes at the end (but perhaps can be leaks & last data losses) - for IHS BDD 09:43 min
            return writer;
        }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, 
            Func<TState, Exception, string> formatter)
        {
            var data = FormatData(logLevel, eventId, state, exception, formatter);
            _queue.Enqueue(data);
        }

        public override IDisposable BeginScope<TState>(TState state)
        {
            Flush();
            return _writer;
        }

        public async override void Flush()
        {
            await Task.Run(() => _queue.Flush());
            if (_queue.QueueItemsCounter > 0)
            {
               
                while (_queue.QueueItemsCounter > 0)
                    await Task.Delay(10);
            }
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
            
        }
    }
}
