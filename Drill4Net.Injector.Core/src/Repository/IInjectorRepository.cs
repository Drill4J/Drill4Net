﻿using System.Diagnostics.CodeAnalysis;

namespace Drill4Net.Injector.Core
{
    public interface IInjectorRepository
    {
        MainOptions Options { get; set; }
        void ValidateOptions();
        void ValidateOptions(MainOptions opts);
        InjectedSolution ReadInjectedTree(string path);
        void WriteInjectedTree(string path, InjectedSolution tree);
        string GetTreeFilePath(InjectedSolution tree);
        string GetTreeFileHintPath(string path);
    }
}