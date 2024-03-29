﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Profiling.Tree;
using Drill4Net.Agent.Abstract;

namespace Drill4Net.Agent.Standard.Tester
{
    /// <summary>
    /// Standart Agent for the Tester app
    /// </summary>
    internal class TesterCommander
    {
        internal TesterTreeInfo TreeInfo { get; }
        private readonly TesterOutputHelper _helper;

        /*************************************************************/

        internal TesterCommander(TesterOutputHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            TreeInfo = new TesterTreeInfo();
        }

        /*************************************************************/

        internal async Task Init()
        {
            _helper.WriteMessage("Please wait for the init...", TesterConstants.COLOR_TEXT);

            TreeInfo.Opts = TesterOptionsHelper.GetOptions();
            var moniker = TreeInfo.Opts.Moniker;
            TreeInfo.TargetPath = TreeInfo.Opts.Directory;
            if(!string.IsNullOrWhiteSpace(moniker))
                TreeInfo.TargetPath = Path.Combine(TreeInfo.TargetPath, moniker);

            //tree's data (in fact, we can use the TestEngine's one)
            AgentInitParameters.TargetDir = Path.Combine(TreeInfo.Opts.Directory, TreeInfo.Opts.Moniker);
            var rep = StandardAgent.Agent.Repository;
            TreeInfo.InjSolution = rep.ReadInjectedTree();

            //info about injected directory
            var injDirs = TreeInfo.InjSolution.GetDirectories();
            TreeInfo.InjDirectory = injDirs.FirstOrDefault(a => string.IsNullOrWhiteSpace(moniker) || a.Name == moniker);
            if (TreeInfo.InjDirectory == null)
                throw new Exception($"Directory in the Tree data not found: [{TreeInfo.TargetPath}]");

            TreeInfo.Points = TreeInfo.InjDirectory.GetAllPoints().ToList();

            //methods
            var methList = TreeInfo.InjDirectory.GetAllMethods().Where(a => !a.IsCompilerGenerated);
            TreeInfo.Methods = new Dictionary<string, InjectedMethod>();
            foreach (var meth in methList)
            {
                var key = meth.Name;
                if (!TreeInfo.Methods.ContainsKey(key))
                    TreeInfo.Methods.Add(key, meth);
            }
            TreeInfo.MethodSorted = MethodsHelper.GetSortedMethods(TreeInfo.Methods);
            TreeInfo.MethodByOrderNumber = MethodsHelper.GetMethodByOrderNumber(TreeInfo.MethodSorted);

            await Task.Delay(1500); //wait for all logs from native dll (Connector)
            _helper.WriteMessage("Let's wait a little longer...", TesterConstants.COLOR_TEXT);
            await Task.Delay(5000); //wait for the admin side init
        }

        internal void Poll()
        {
            while (true)
            {
                _helper.WriteMessage("\nInput:");
                var input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (input == "q" || input == "Q")
                    break;
                try
                {
                    ProcessInput(input);
                }
                catch (Exception ex)
                {
                    _helper.WriteMessage($"error -> {ex.Message}", TesterConstants.COLOR_ERROR);
                }
            }

            Console.ReadKey(true);
        }

        /// <summary>
        /// Sends to the Admin side the points.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private bool ProcessInput(string input)
        {
            input = input.Trim();
            return input switch
            {
                "?" or "help" => _helper.PrintMenu(),
                "tree" or "list" => _helper.PrintTree(TreeInfo.MethodSorted),
                "info" => _helper.PrintTreeInfo(TreeInfo),
                "save" => _helper.SaveTreeData(TreeInfo.MethodSorted, TreeInfo.Opts),
                _ => CallMethod(input)
            };
        }

        private bool CallMethod(string callData)
        {
            //get info
            if (string.IsNullOrWhiteSpace(callData))
            {
                _helper.WriteMessage("No input", TesterConstants.COLOR_ERROR);
                return false;
            }
            if (callData.Contains("await "))
                callData = callData.Replace("await ", null);
            if (callData.EndsWith(";"))
                callData = callData[0..^1];
            callData = callData.Replace("(", " ").Replace(")", null);
            var spInd = callData.IndexOf(" ");
            var name = spInd == -1 ? callData : callData.Substring(0, spInd).Trim();
            var parsS = spInd == -1 ? null : callData[spInd..];

            //by order number?
            if (int.TryParse(name, out int number))
            {
                if (!TreeInfo.MethodByOrderNumber.ContainsKey(number))
                {
                    _helper.WriteMessage($"No such order number: {number}", TesterConstants.COLOR_ERROR);
                    return false;
                }
                name = TreeInfo.MethodByOrderNumber[number].Name;
            }

            //by name
            if (!TreeInfo.Methods.ContainsKey(name))
            {
                _helper.WriteMessage("No such method", TesterConstants.COLOR_ERROR);
                return false;
            }

            //parameters
            var pars = new List<string>();
            if (parsS != null)
            {
                foreach (var par in parsS.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(par))
                        pars.Add(par.Trim());
                }
            }

            //getting MethodInfo
            var method = TreeInfo.Methods[name];
            MethodInfo methInfo;
            object target;
            try
            {
                _helper.WriteMessage($"Calling: {method.FullName}", TesterConstants.COLOR_TEXT);
                //
                var asmPath = Path.Combine(TreeInfo.TargetPath, method.AssemblyName);
                var asms = AppDomain.CurrentDomain.GetAssemblies();
                var asm = Array.Find(asms, a => a.ManifestModule.Name == method.AssemblyName);
                if(asm == null)
                    asm = Assembly.LoadFrom(asmPath);
                //
                var typeName = method.BusinessType;
                var type = asm.GetType(typeName);
                methInfo = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (methInfo == null)
                {
                    _helper.WriteMessage($"Method {name} not found", TesterConstants.COLOR_ERROR);
                    return false;
                }
                target = Activator.CreateInstance(asm.FullName, typeName).Unwrap();
            }
            catch (Exception ex)
            {
                _helper.WriteMessage($"Error of retrieving MethodInfo for [{method.FullName}] by reflection:\n{ex.Message}", TesterConstants.COLOR_ERROR);
                return false;
            }

            //calling
            try
            {
                var pars2 = ParametersConverter.ConvertToRealParameters(pars, method.Signature.Parameters);
                methInfo.Invoke(target, pars2);
            }
            catch (Exception ex)
            {
                _helper.WriteMessage($"Error of the method [{method.FullName}] calling: [{callData}]:\n{ex.Message}", TesterConstants.COLOR_ERROR);
                return false;
            }
            return true;
        }
    }
}
