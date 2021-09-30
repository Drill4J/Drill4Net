using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;
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

        /// <summary>
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        public string EmergencyLogDir { get; }

        private static ConcurrentDictionary<string, bool> _probes;
        private readonly Pinger _pinger;
        private readonly AssemblyResolver _resolver;

        private static readonly Logger _logger;
        private bool _disposed;

        /***********************************************************************************/

        static DataTransmitter()
        {
            AbstractRepository.PrepareEmergencyLogger();
            Log.Debug($"Enter to {nameof(DataTransmitter)} .cctor");

            ITargetSenderRepository rep = new TransmitterRepository();
            Log.Debug("Repository created.");

            var extrasData = new Dictionary<string, object> { { "TargetSession", rep.TargetSession } };
            _logger = new TypedLogger<DataTransmitter>(rep.Subsystem, extrasData);

            Transmitter = new DataTransmitter(rep); //what is loaded into the Target process and used by the Proxy class
            Transmitter.SendTargetInfo(rep.GetTargetInfo());

            _logger.Debug("Initialized.");
        }

        public DataTransmitter(ITargetSenderRepository rep)
        {
            _logger.Debug($"Creating object of {nameof(DataTransmitter)}...");

            //TODO: find out - on IHS adoption it falls
            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
            //AppDomain.CurrentDomain.ResourceResolve += CurrentDomain_ResourceResolve;
            //_resolver = new AssemblyResolver();

            EmergencyLogDir = FileUtils.EmergencyDir;
            _probes = new ConcurrentDictionary<string, bool>();

            //TODO: factory
            InfoSender = new TargetInfoKafkaSender(rep); //sender the target info
            ProbeSender = new ProbeKafkaSender(rep); //sender the data of probes

            var pingSender = new PingKafkaSender(rep);
            _pinger = new Pinger(rep, pingSender);

            _logger.Debug($"Object of {nameof(DataTransmitter)} is created");
        }

        ~DataTransmitter()
        {
            Dispose(false);
        }

        /************************************************************************************/

        #region Init
        //TODO: find out - on IHS adoption it falls
        //private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        //{
        //    CommonUtils.LogFirstChanceException(EmergencyLogDir, _logPrefix, e.Exception);
        //}

        //private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    return CommonUtils.TryResolveAssembly(EmergencyLogDir, _logPrefix, args, _resolver, null); //TODO: use BanderLog!
        //}

        //private Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        //{
        //    return CommonUtils.TryResolveResource(EmergencyLogDir, _logPrefix, args, _resolver, null); //TODO: use BanderLog!
        //}

        //private Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        //{
        //    return CommonUtils.TryResolveType(EmergencyLogDir, _logPrefix, args, null); //TODO: use BanderLog!
        //}
        #endregion

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
            if (!_probes.TryAdd(data, true))
                return;
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

        public static void DoCommand(int command, string data)
        {
            _logger.Info($"Command: {command} -> {data}");

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
