﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// The injected method
    /// </summary>
    [Serializable]
    public class InjectedMethod : InjectedEntity
    {
        /// <summary>
        /// Gets or sets the method's signature.
        /// </summary>
        /// <value>
        /// The method's signature.
        /// </value>
        public MethodSignature Signature { get; set; }

        /// <summary>
        /// Some metadata about the current method
        /// </summary>
        public MethodSource Source { get; set; }

        /// <summary>
        /// Is this method compiler generated?
        /// </summary>
        public bool IsCompilerGenerated => Source.MethodType == MethodType.CompilerGenerated;

        /// <summary>
        /// Compiler generated info if the method is generated by Compiler
        /// </summary>
        public CompilerGeneratedInfo CGInfo { get; }

        /// <summary>
        /// Indexes of the IL code's original instructions for the current method's callees
        /// </summary>
        public Dictionary<string, int> CalleeOrigIndexes { get; set; }

        /// <summary>
        /// End-to-end business indexes and corresponding uids of injected cross-points in the
        /// IL code's instructions for current method itself and for its callees located in
        /// one logical array
        /// </summary>
        public List<(int Index,string Uid)> End2EndBusinessIndexes { get; set; }

        /// <summary>
        /// Name of the business type (for the compiler generated methods)
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// Name of the business method (for the compiler generated methods)
        /// </summary>
        public string BusinessMethod => GetBusinessMethod(); //it's better as a method because this info during the injector's work can be changed

        /// <summary>
        /// Count of instructions in various 'business parts' of the IL code
        /// (including compiler generated classes and functions) at the own level
        /// of hierarchy of calls CG members
        /// </summary>
        public int BusinessSize { get; set; } = -1;

        /// <summary>
        /// Count of only own 'business parts' of the IL code
        /// </summary>
        public int OwnBusinessSize { get; set; } = -1;

        /// <summary>
        /// Cross-points of method's code
        /// </summary>
        public IEnumerable<CrossPoint> Points => Filter(typeof(CrossPoint), false).Cast<CrossPoint>();

        /********************************************************************/

        public InjectedMethod(string assemblyName, string businessTypeName, string fullName, MethodSource sourceType)
        {
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
            Source = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            BusinessType = businessTypeName ?? throw new ArgumentNullException(nameof(businessTypeName));
            FullName = fullName;
            Signature = ParseSignature(fullName);
            Name = Signature.Name;
            if (sourceType.MethodType == MethodType.CompilerGenerated)
                CGInfo = new CompilerGeneratedInfo();
            CalleeOrigIndexes = new Dictionary<string, int>();
        }

        /********************************************************************/

        internal static MethodSignature ParseSignature(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return new MethodSignature();
            
            //TODO: regex !!! AAAAAAAAAA!!!!
            //Fullname example: System.String Drill4Net.Target.Common.AbstractGen`1::GetDesc(System.Boolean)
            string ns = null; string type = null; string retType = null;
            string pars = null; string name;
            //
            if (!fullName.Contains("::")) //it's exactly short name
            {
                name = fullName;
            }
            else
            {
                var s = fullName;
                if (fullName.Contains(" ")) //return param exists?
                {
                    var ar = s.Split(' ');
                    retType = ar[0];
                    s = ar[1];
                }
                var ar1 = s.Split(':');
                var nsAr = ar1[0].Split('.');
                for (var i = 0; i < nsAr.Length - 1; i++)
                    ns += nsAr[i] + ".";
                ns = ns?.Remove(ns.Length - 1, 1);
                type = nsAr[nsAr.Length - 1];

                var ar2 = ar1[2].Split('(');
                name = ar2[0];

                pars = ar2[1];
                pars = pars.Length > 1 ? pars.Remove(pars.Length - 1, 1) : null;
            }

            return new MethodSignature(ns, type, retType, name, pars);
        }

        /// <summary>
        /// Gets the business method by its current known data.
        /// This is used during the injection process
        /// </summary>
        /// <returns></returns>
        internal virtual string GetBusinessMethod()
        {
            var method = this;
            while (true)
            {
                var cgInfo = method.CGInfo;
                if (cgInfo == null)
                    return method.FullName;
                if (cgInfo.Caller == null)
                    return cgInfo.FromMethod ?? method.FullName;
                method = cgInfo.Caller;
            }
        }

        //public InjectedType FindBusinessType(Dictionary<InjectedSimpleEntity, InjectedSimpleEntity> parentMap,
        //    InjectedMethod forEntity)
        //{
        //    if (parentMap == null)
        //        throw new ArgumentNullException(nameof(parentMap));
        //    if (forEntity == null)
        //        throw new ArgumentNullException(nameof(forEntity));
        //    //
        //    InjectedType type = null;
        //    InjectedSimpleEntity key = forEntity;
        //    while (parentMap.ContainsKey(key))
        //    {
        //        type = parentMap[key] as InjectedType;
        //        if (type is { IsCompilerGenerated: false })
        //            break;
        //        key = type;
        //    }
        //    return type;
        //}

        public override string ToString()
        {
            return $"{base.ToString()}M: {FullName}";
        }
    }
}
