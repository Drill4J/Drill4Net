using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Drill4Net.BanderLog
{
    public class Logger : ILogger, IBanderLogger
    {
        public string Subsystem { get; }
        public string Category { get; }
        public Dictionary<string, object> Extras { get; }

        public string ExtrasString { get; private set; }

        private readonly JsonSerializerSettings _serOpts;

        /**********************************************************************************************/

        public Logger(string category = null, string subsystem = null) : this(category, subsystem,  null)
        {
        }

        public Logger(string category, string subsystem, Dictionary<string, object> extras)
        {
            Category = category;
            Subsystem = subsystem;
            Extras = extras ?? new Dictionary<string, object>();

            //serialization 
            //TODO: get options from parameters, etc
            //https://www.newtonsoft.com/json/help/html/SerializationSettings.htm
            _serOpts = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            };
            RefreshExtrasInfo();
        }

        /**********************************************************************************************/

        public virtual ILogManager GetManager() => BanderLog.Log.Manager;

        public void RefreshExtrasInfo()
        {
            if (Extras?.Count > 0)
                ExtrasString = JsonConvert.SerializeObject(Extras, Formatting.None, _serOpts); //as one object yet
            else
                ExtrasString = null;
        }

        #region Specific
        public void Trace<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Trace(this, message, exception, callerMethod);
        }

        public void Debug<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Debug(this, message, exception, callerMethod);
        }

        //public void Debug(string template, params object[] data)
        //{
        //    //BanderLog.Log.Debug(this, message, exception, callerMethod);
        //}

        public void Info<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Info(this, message, exception, callerMethod);
        }

        public void Warning<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Warning(this, message, exception, callerMethod);
        }

        public void Error<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Error(this, message, exception, callerMethod);
        }

        public void Error(Exception exception, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Error<string>(this, null, exception, callerMethod);
        }

        public void Fatal<TState>(TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Fatal(this, message, exception, callerMethod);
        }
        #endregion
        #region Write
        public void Write<TState>(LogLevel logLevel, TState message, Exception exception = null, [CallerMemberName] string callerMethod = "")
        {
            BanderLog.Log.Write(logLevel, this, message, exception, callerMethod);
        }

        public void Write<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            BanderLog.Log.Write(logLevel, eventId, state, exception, formatter);
        }
        #endregion
        #region Interface ILogger
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            GetManager()?.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return GetManager()?.IsEnabled(logLevel) == true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return GetManager()?.BeginScope(state);
        }
        #endregion

        public override string ToString()
        {
            return $"{Subsystem}: {Category}";
        }
    }
}