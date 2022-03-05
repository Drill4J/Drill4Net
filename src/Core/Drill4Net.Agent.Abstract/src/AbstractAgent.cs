using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Repository;

namespace Drill4Net.Agent.Abstract
{
    /// <summary>
    /// Handler for <see cref="AbstractAgent.Initialized"/>
    /// </summary>
    public delegate void AgentInitializedHandler();

    /// <summary>
    /// Abstract agent with basic functionality. Concrete agent with repository should inherit next level of agents: "AbstractAgent[TRep] where TRep : AgentRepository"
    /// </summary>
    public abstract class AbstractAgent
    {
        /// <summary>
        /// The Agent is initialized
        /// </summary>
        public event AgentInitializedHandler Initialized;

        /// <summary>
        /// Is the Agent initialized?
        /// </summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>
        /// Directory for the emergency logs out of scope of the common log system
        /// </summary>
        public static string EmergencyLogDir { get; }

        protected ICommunicator _comm;
        private readonly AssemblyResolver _resolver;

        /**************************************************************************/

        static AbstractAgent()
        {
            EmergencyLogDir = LoggerHelper.GetDefaultLogDir();
            AbstractRepository.PrepareEmergencyLogger();
        }

        protected AbstractAgent()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
            AppDomain.CurrentDomain.ResourceResolve += CurrentDomain_ResourceResolve;
            _resolver = new AssemblyResolver();

            #region TEST assembly resolving
            //var ver = "Microsoft.Data.SqlClient.resources, Version=2.0.20168.4, Culture=en-US, PublicKeyToken=23ec7fc2d6eaa4a5";
            //var ver = "System.Text.Json, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

            //var ver = "System.Private.Xml.resources, Version=4.0.2.0, Culture=en-US, PublicKeyToken=cc7b13ffcd2ddd51";
            //var reqPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.16\System.Private.Xml.dll";

            //var ver = "Drill4Net.Target.Common.VB, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            //var reqPath = @"d:\Projects\EPM-D4J\Drill4Net\build\bin\Debug\Tests\TargetApps.Injected\Drill4Net.Target.Net50.App\net5.0\";
            //var asm = _resolver.Resolve(ver, reqPath);

            //var asm = _resolver.ResolveResource(@"d:\Projects\IHS-bdd.Injected\de-DE\Microsoft.Data.Tools.Schema.Sql.resources.dll", "Microsoft.Data.Tools.Schema.Sql.Deployment.DeploymentResources.en-US.resources");
            #endregion
        }

        /**************************************************************************/

        #region Init
        protected void RaiseInitilizedEvent()
        {
            Initialized?.Invoke();
        }

        internal protected void AddDependencies(List<string> dirs)
        {
            if(dirs?.Count > 0)
                _resolver.SearchDirs.AddRange(dirs);
        }

        internal protected void AddDependencies(string root)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                return;
            AddDependencies(Directory.GetDirectories(root).ToList());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CommonUtils.LogUnhandledException(EmergencyLogDir, nameof(AbstractAgent), e.ExceptionObject?.ToString());
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return CommonUtils.TryResolveAssembly(EmergencyLogDir, nameof(AbstractAgent), args, _resolver, null); //TODO: use BanderLog!
        }

        private Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            return CommonUtils.TryResolveResource(EmergencyLogDir, nameof(AbstractAgent), args, _resolver, null); //TODO: use BanderLog!
        }

        private Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            return CommonUtils.TryResolveType(EmergencyLogDir, nameof(AbstractAgent), args, null); //TODO: use BanderLog!
        }
        #endregion

        /// <summary>
        /// Register the cross-pont's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        public abstract void Register(string data);

        public abstract void RegisterWithContext(string data, string ctx);

        /// <summary>
        /// Async register the cross-pont's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        public Task RegisterAsync(string data)
        {
            return Task.Run(() => Register(data));
        }

        /// <summary>
        /// Async register the cross-point's probe data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="ctx"></param>
        public Task RegisterWithContextAsync(string data, string ctx)
        {
            return Task.Run(() => RegisterWithContext(data, ctx));
        }

        public static string GetDefaultConnectorLogFilePath()
        {
            return Path.Combine(LoggerHelper.GetDefaultLogDir(), AgentConstants.CONNECTOR_LOG_FILE_NAME);
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Abstract Agent collecting probe data of the cross-point in instrumented Target
    /// </summary>
    public abstract class AbstractAgent<TRep> : AbstractAgent where TRep : AgentRepository
    {
        /// <summary>
        /// Repository for Agent
        /// </summary>
        public TRep Repository { get; protected set; }
    }
}
