using System;
using Drill4Net.Common;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Kafka;

namespace Drill4Net.Agent.Transmitter
{
    /// <summary>
    /// Entity which is loaded by Proxy to Target's process and next transmits
    /// the probe's data from it to the real Agent located in separate service 
    /// (direct or, e.g. through Kafka as middleware)
    /// </summary>
    public class DataTransmitter : IDisposable
    {
        public static DataTransmitter Transmitter { get; }

        public ITargetInfoSender InfoSender { get; }
        public IProbeSender ProbeSender { get; }

        private readonly Pinger _pinger;
        private bool _disposed;

        /***********************************************************************************/

        static DataTransmitter()
        {
            ITargetSenderRepository rep = new TransmitterRepository();
            ITargetInfoSender infoSender = new TargetInfoKafkaSender(rep); //concrete sender the target info to the middleware
            IProbeSender probeSender = new ProbeKafkaSender(rep); //concrete sender the data of probes to the middleware
            Transmitter = new DataTransmitter(infoSender, probeSender); //what is loaded into the Target process and used by the Proxy class

            Transmitter.SendTargetInfo(rep.GetTargetInfo());
        }

        public DataTransmitter(ITargetInfoSender infoSender, IProbeSender probeSender)
        {
            InfoSender = infoSender ?? throw new ArgumentNullException(nameof(infoSender));
            ProbeSender = probeSender ?? throw new ArgumentNullException(nameof(probeSender));
            //_pinger = new Pinger()
        }

        ~DataTransmitter()
        {
            Dispose(false);
        }

        /************************************************************************************/

        internal void SendTargetInfo(byte[] info)
        {
            InfoSender.SendTargetInfo(info);
        }

        /// <summary>
        /// Transmits the specified probe from the Proxy class injected into Target to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        public static void Transmit(string data)
        {
            var ctx = Contexter.GetContextId();
            Transmitter.SendProbe(data, ctx);
        }

        /// <summary>
        /// Sends the specified probe to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        /// <param name="ctx">The context of data (user, process, worker, etc)</param>
        public int SendProbe(string data, string ctx)
        {
            return ProbeSender.SendProbe(data, ctx);
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _pinger?.Dispose();
                    ProbeSender?.Dispose();
                    InfoSender?.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //.....

                // Note disposing has been done.
                _disposed = true;
            }
        }
        #endregion
    }
}
