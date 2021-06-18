using System;
using Mono.Cecil;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Engine
{
    /// <summary>
    /// Helper for working with info for a injecting Proxy class
    /// </summary>
    public static class ProxyHelper
    {
        /// <summary>
        /// Get the type namespace for the injected proxy class.
        /// </summary>
        /// <returns></returns>
        public static string CreateProxyNamespace()
        {
            //must be unique for each target asm
            return $"Injection_{Guid.NewGuid()}".Replace("-", null);
        }

        /// <summary>
        /// Creates the proxy method's call reference for the injecting into the target's code.
        /// </summary>
        /// <param name="asmCtx">The assembly context.</param>
        /// <param name="opts">The Injector options.</param>
        /// <returns></returns>
        public static MethodReference CreateProxyMethodReference(AssemblyContext asmCtx, InjectorOptions opts)
        {
            //We will use proxy class (with cached Reflection) leading to real profiler.
            //Proxy will be injected in each target assembly - let construct the calling of it's method
            var module = asmCtx.Module;
            var proxyReturnTypeRef = module.TypeSystem.Void;
            var proxyTypeRef = new TypeReference(asmCtx.ProxyNamespace, opts.Proxy.Class, module, module);
            var proxyMethRef = new MethodReference(opts.Proxy.Method, proxyReturnTypeRef, proxyTypeRef);
            var strPar = new ParameterDefinition("data", ParameterAttributes.None, module.TypeSystem.String);
            proxyMethRef.Parameters.Add(strPar);
            return proxyMethRef;
        }
    }
}
