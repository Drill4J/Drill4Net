﻿using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Core.Repository;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Messaging;
using Drill4Net.Agent.Messaging.Kafka;
using Drill4Net.BanderLog.Sinks.File;

[assembly: InternalsVisibleTo("Drill4Net.Agent.Transmitter.Debug")]

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

        public ITargetInfoSender TargetSender { get; }
        public IProbeSender ProbeSender { get; }
        public ICommandSender CommandSender { get; }

        /// <summary>
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        public string EmergencyLogDir { get; }

        /// <summary>
        /// In fact, this is a limiter to reduce the flow of sending probes 
        /// to the administrator's side
        /// </summary>
        private static ConcurrentDictionary<string, bool> _probes;

        private static readonly ContextDispatcher _ctxDisp;
        private readonly Pinger _pinger;
        private readonly AssemblyResolver _resolver;

        private static readonly Logger _logger;
        private static readonly FileSink _probeLogger;
        private static readonly bool _writeProbesToFile;

        private bool _disposed;

        /***********************************************************************************/

        static DataTransmitter()
        {
            AbstractRepository.PrepareEmergencyLogger();
            Log.Trace($"Enter to {nameof(DataTransmitter)} .cctor");

            var rep = new TransmitterRepository();
            Log.Debug($"{nameof(TransmitterRepository)} created.");

            var extras = new Dictionary<string, object> { { "TargetSession", rep.TargetSession } };
            _logger = new TypedLogger<DataTransmitter>(rep.Subsystem, extras);

            Transmitter = new DataTransmitter(rep); //what is loaded into the Target process and used by the Proxy class

            _logger.Debug("Getting & sending the Target's info");
            Transmitter.SendTargetInfo(rep.GetTargetInfo());

            _ctxDisp = new ContextDispatcher(rep.Options.PluginDir, rep.Subsystem);

            const int delay = 12;
            _logger.Debug($"Waiting for {delay} seconds...");
            Thread.Sleep(delay * 1000); //here we need "sync waiting" for the Agent Worker init

            //debug
            var debug = rep.Options.Debug;
            _writeProbesToFile = debug?.Disabled == false && debug.WriteProbes;
            if (_writeProbesToFile)
            {
                var probeLogfile = Path.Combine(FileUtils.GetCommonLogDirectory(FileUtils.EntryDir), "probes.log");
                _logger.Debug($"Probes writing to [{probeLogfile}]");
                if (File.Exists(probeLogfile))
                    File.Delete(probeLogfile);
                _probeLogger = new FileSink(probeLogfile);
            }

            _logger.Debug("Initialized.");
        }

        public DataTransmitter(ITargetSenderRepository rep)
        {
            _logger.Debug($"{nameof(DataTransmitter)} singleton is creating...");

            //TODO: find out - on IHS adoption it falls
            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
            //AppDomain.CurrentDomain.ResourceResolve += CurrentDomain_ResourceResolve;
            //_resolver = new AssemblyResolver();

            EmergencyLogDir = FileUtils.EmergencyDir;
            _probes = new ConcurrentDictionary<string, bool>();

            //TODO: factory
            TargetSender = new TargetInfoKafkaSender(rep);
            ProbeSender = new ProbeKafkaSender(rep);
            CommandSender = new CommandKafkaSender(rep);

            var pingSender = new PingKafkaSender(rep);
            _pinger = new Pinger(rep, pingSender);

            _logger.Debug($"{nameof(DataTransmitter)} singleton is created");
        }

        ~DataTransmitter()
        {
            Dispose(false);
        }

        /************************************************************************************/

        #region Resolving
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
            _logger.Info("Sending Target's info");
            TargetSender.SendTargetInfo(info);
        }

        #region Transmit probe
        /// <summary>
        /// Transmits the specified probe from the Proxy class injected into Target to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        public static void Transmit(string data)
        {
            //unfortunately, caching is wrong techique here - maybe later...
            //if (!_probes.TryAdd(data, true))
                //return;
            var ctx = _ctxDisp.GetContextId();
            TransmitWithContext(data, ctx);
        }

        /// <summary>
        /// Transmits the specified probe from the Proxy class injected into Target to the middleware.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        /// <param name="ctx">context of the probe</param>
        public static void TransmitWithContext(string data, string ctx)
        {
            //unfortunately, caching is wrong techique here
            //if (!_probes.TryAdd(data, true))
               //return;
            Transmitter.SendProbe(data, ctx);
        }

        /// <summary>
        /// Sends the specified probe to the middleware.
        /// It is internal method for the debug purposes.
        /// </summary>
        /// <param name="data">The cross-point data.</param>
        /// <param name="ctx">The context of data (user, process, worker, etc)</param>
        internal int SendProbe(string data, string ctx)
        {
            if (_writeProbesToFile)
                _probeLogger.Log(LogLevel.Trace, $"{ctx} -> [{data}]");

            return ProbeSender.SendProbe(data, ctx);
        }
        #endregion
        #region Command
        public void ExecCommand(int command, string data)
        {
            _logger.Info($"Command: [{command}] -> {data}");
            ProbeSender.Flush(); //we have to guarantee the delivery of the previous probes
            CommandSender.SendCommand(command, data);
            Log.Flush();
        }

        public static void DoCommand(int command, string data)
        {
            Transmitter.ExecCommand(command, data);
        }
        #endregion
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
                    TargetSender?.Dispose();
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
