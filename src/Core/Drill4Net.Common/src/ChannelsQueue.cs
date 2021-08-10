using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Drill4Net.Common
{
    //https://michaelscodingspot.com/performance-of-producer-consumer/

    /// <summary>
    /// Channel's queue for fast concurrent processing of the string data by given action
    /// </summary>
    public class ChannelsQueue<T>
    {
        private readonly ChannelReader<T> _reader;
        private readonly ChannelWriter<T> _writer;
        private int _queueItemsCounter;


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
                    }
                }
            });
        }

        /**************************************************************************/

        public void Enqueue(T data)
        {
            IncrementItemsCounter();
            _writer.TryWrite(data); //TODO: if false, wtite to local queue, then repeat attempt 
        }
        public void DecrementItemsCounter()
        {
            _queueItemsCounter = Interlocked.Decrement(ref _queueItemsCounter);
        }
        public void IncrementItemsCounter()
        {
            _queueItemsCounter = Interlocked.Increment(ref _queueItemsCounter);
        }
        public void AddItemsCounter( int amount)
        {
            _queueItemsCounter = Interlocked.Add(ref _queueItemsCounter, amount);
        }

        public void Stop()
        {
            _writer.Complete();
        }

        public async void Flush()
        {
            Stop();

            if (_queueItemsCounter > 0)
            {
                while (_queueItemsCounter > 0)
                    await Task.Delay(10);
            }
            else
            {
                await Task.Delay(250);
            }

        }
    }
}
