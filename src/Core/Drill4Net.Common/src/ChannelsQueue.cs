using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace Drill4Net.Common
{
    //https://michaelscodingspot.com/performance-of-producer-consumer/

    /// <summary>
    /// Channel's queue for fast concurrent processing of the string data by given action
    /// </summary>
    public class ChannelsQueue<T>
    {
        /// <summary>
        /// Gets the queue items current counter.
        /// </summary>
        /// <value>
        /// The queue items current counter.
        /// </value>
        public int ItemCount { get => _queueItemCount; }

        private readonly ChannelReader<T> _reader;
        private readonly ChannelWriter<T> _writer;
        private int _queueItemCount;

        /**************************************************************************/

        public ChannelsQueue(Action<T> job)
        {
            var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions() { SingleReader = true });
            _reader = channel.Reader;
            _writer = channel.Writer;

            Task.Run(async() =>
            {
                while (await _reader.WaitToReadAsync())
                {
                    // Fast loop around available data
                    while (_reader.TryRead(out var data))
                    {
                        job.Invoke(data);
                        _queueItemCount--;
                    }
                }
            });
        }

        /**************************************************************************/

        public void Enqueue(T data)
        {
            IncrementItemCounter();
            _writer.TryWrite(data); //TODO: if false, wtite to local queue, then repeat attempt? 
        }

        public int DecrementItemCounter()
        {
            return Interlocked.Decrement(ref _queueItemCount);
        }

        public int IncrementItemCounter()
        {
            return Interlocked.Increment(ref _queueItemCount);
        }

        public void Stop()
        {
            _writer.Complete();
        }
    }
}
