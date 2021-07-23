using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Drill4Net.Agent.RnD
{
    //https://michaelscodingspot.com/performance-of-producer-consumer/

    /// <summary>
    /// Channel's queue for fast concurrent processing of the string data by given action
    /// </summary>
    public class ChannelsQueue
    {
        private readonly ChannelWriter<string> _writer;

        /**************************************************************************/

        public ChannelsQueue(Action<string> job)
        {
            var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions() { SingleReader = true });
            var reader = channel.Reader;
            _writer = channel.Writer;

            Task.Run(async () =>
            {
                while (await reader.WaitToReadAsync())
                {
                    // Fast loop around available data
                    while (reader.TryRead(out var data))
                    {
                        job.Invoke(data);
                    }
                }
            });
        }

        /**************************************************************************/

        public void Enqueue(string data)
        {
            _writer.TryWrite(data);
        }

        public void Stop()
        {
            _writer.Complete();
        }
    }
}
