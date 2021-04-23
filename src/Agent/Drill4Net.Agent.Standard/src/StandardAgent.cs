using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;
using Drill4Net.Agent.Abstract;
using Drill4Net.Common;

namespace Drill4Net.Agent.Standard
{
    public class StandardAgent : AbstractAgent
    {
        private static readonly AgentReceiver _receiver;
        private static readonly AgentSender _sender;

        private static readonly StandardAgentRepository _rep;

        /*****************************************************************************/

        static StandardAgent()
        {
            PrepareLogger();
            Log.Debug("Initializing...");

            try
            {
                _rep = new StandardAgentRepository();
                //
                var communicator = _rep.GetCommunicator();
                _receiver = new(communicator);
                _receiver.SessionStarted += SessionStarted;
                _receiver.SessionFinished += SessionFinished;
                _receiver.SessionCancelled += SessionCancelled;
                _receiver.AllSessionsCancelled += AllSessionsCancelled;
                //
                _sender = new(communicator);

                //1. Classes to admin side
                var entities = _rep.GetEntities();
                _sender.SendClassesDataMessage(entities);

                //2. "Initialized" message to admin side
                _sender.SendInitializedMessage();
                //
                Log.Debug("Initialized.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Error of {nameof(StandardAgent)} initializing");
            }
        }

        /*****************************************************************************/

        #region Session
        private static void SessionStarted(string sessionUid, string testType, bool isRealTime, long startTime)
        {
            _rep.SessionStarted(sessionUid, testType, isRealTime, startTime);
        }

        private static void SessionFinished(string sessionUid, long finishTime)
        {
            _rep.SessionFinished(sessionUid, finishTime);
        }

        private static void AllSessionsCancelled()
        {
            throw new NotImplementedException();
        }

        private static void SessionCancelled(string sessionUid, long cancelTime)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Register
        // ReSharper disable once MemberCanBePrivate.Global
        public static void RegisterStatic(string data)
        {
            try
            {
                #region Checks
                if (string.IsNullOrWhiteSpace(data))
                {
                    Log.Error("Data is empty");
                    return;
                }
                //
                var ar = data.Split('^');
                if (ar.Length < 4)
                {
                    Log.Error($"Bad format of input: {data}");
                    return;
                }
                #endregion

                var probeUid = ar[0];
                //var asmName = ar[1];
                //var funcName = ar[2];
                //var probe = ar[3];

                _rep.GetCoverageDispather().RegisterCoverage(probeUid);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{data}");
            }
        }

        public override void Register(string data)
        {
            RegisterStatic(data);
        }
        #endregion

        public static void PrepareLogger()
        {
            var cfg = new LoggerHelper().GetBaseLoggerConfiguration();
            //common log
            cfg.WriteTo.File(Path.Combine(FileUtils.GetCommonLogDirectory(@"..\..\"), $"{nameof(StandardAgent)}.log"));
            Log.Logger = cfg.CreateLogger();
        }
    }
}