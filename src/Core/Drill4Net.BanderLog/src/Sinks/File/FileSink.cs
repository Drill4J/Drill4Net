using System;
using System.IO;
using System.Threading.Tasks;
using Drill4Net.Common;
using Microsoft.Extensions.Logging;

namespace Drill4Net.BanderLog.Sinks.File
{
    public class FileSink : AbstractTextSink
    {
        private const string NAME_DEFAULT = "log.txt";

        private readonly string _filepath;
        private readonly ChannelsQueue<string> _queue;
        private readonly StreamWriter _writer;

        /*****************************************************************************/

        public FileSink(string filepath = null)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                filepath = Path.Combine(FileUtils.GetEntryDir(), NAME_DEFAULT);
            _filepath = filepath;
        #pragma warning disable DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
            _writer = System.IO.File.AppendText(_filepath); //writes to memory and flushes at the end (but perhaps can be leaks & last data losses) - for IHS BDD 09:43 min
            //Action<string> action = (string str) => File.AppendAllLines(_filepath, new string[] { str }); //opens & closes file each time - for IHS BDD 18:08 min
            Action<string> action = (string str) =>
            {
                _writer.WriteLine(str);
            };
            _queue = new ChannelsQueue<string>(action);
        #pragma warning restore DF0025 // Marks undisposed objects assinged to a field, originated from method invocation.
        }

        /*****************************************************************************/

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
            _writer.Close();
        }
    }
}
