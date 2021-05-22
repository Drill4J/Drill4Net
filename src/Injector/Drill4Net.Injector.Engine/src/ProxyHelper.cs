﻿using System;
using Mono.Cecil;
using Drill4Net.Common;
using Drill4Net.Injector.Core;

namespace Drill4Net.Injector.Engine
{
    public static class ProxyHelper
    {
        public static string CreateProxyNamespace()
        {
            //must be unique for each target asm
            return $"Injection_{Guid.NewGuid()}".Replace("-", null);
        }

        public static MethodReference CreateProxyMethodReference(AssemblyContext asmCtx, InjectorOptions opts)
        {
            //we will use proxy class (with cached Reflection) leading to real profiler
            //proxy will be inject in each target assembly - let construct the calling of it's method
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
