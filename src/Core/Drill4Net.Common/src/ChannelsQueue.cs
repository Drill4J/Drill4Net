﻿using System;
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
            _writer.TryWrite(data); //TODO: if false, wtite to local queue, then repeat attempt 
        }

        public void Stop()
        {
            _writer.Complete();
        }

        public async void Flush()
        {
            Stop();

            if (_reader.CanCount)
            {
                while (_reader.Count > 0)
                    await Task.Delay(10);
            }
            else
            {
                await Task.Delay(250);
            }
        }
    }
}
