﻿using System.Threading.Tasks;
using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Interface of Injector of instrumenting code called by Target for Agent
    /// </summary>
    public interface IAssemblyInjector
    {
        /// <summary>
        /// Concrete strategy of instrumenting code injections into Target
        /// </summary>
        CodeHandlerStrategy Strategy { get; }

        List<IInjectorPlugin> Plugins { get; }

        /// <summary>
        /// Inject assembly by specified contexts
        /// </summary>
        /// <param name="runCtx"></param>
        /// <param name="asmCtx"></param>
        Task Inject(RunContext runCtx, AssemblyContext asmCtx);
    }
}