using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Injector.Engine
{
    public class InjectorRepository : IInjectorRepository
    {
        //TODO: to parameters!
        private readonly string _defaultDirectory =
            //@"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Target.Net50\bin\Debug\net5.0\";
            @"d:\Projects\EPM-D4J\!!_exp\Injector.Net\Target.Net48\bin\Debug\";

        private readonly string _destinationFolderPostfix = "Injected";

        /*******************************************************************/

        public InjectOptions CreateOptions([NotNull] string[] args)
        {
            string sourceDir = string.Empty;
            string destDir = string.Empty;

            if (args.Length > 0)
                sourceDir = args[0];
            if (args.Length > 1 && args[1].Contains("//"))
                sourceDir = args[1];

            if (string.IsNullOrWhiteSpace(sourceDir))
                sourceDir = _defaultDirectory;
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory not found: [{sourceDir}]");

            if (string.IsNullOrWhiteSpace(destDir))
                destDir = GetInjectedDirectoryName(sourceDir);

            //TODO: collect all parameters from args

            var opts = new InjectOptions
            {
                SourceDirectory = sourceDir,
                DestinationDirectory = destDir,
            };
            return opts;
        }

        public string GetInjectedDirectoryName([NotNull] string original)
        {
            return $"{Path.GetDirectoryName(original)}.{_destinationFolderPostfix}";
        }

        public void ValidateOptions([NotNull] InjectOptions opts)
        {
            if (string.IsNullOrEmpty(opts.SourceDirectory))
                throw new Exception("Source directory name is empty");
            if (!Directory.Exists(opts.SourceDirectory))
                throw new DirectoryNotFoundException("Destination directory does not exists");
            //
            if (string.IsNullOrEmpty(opts.DestinationDirectory))
                throw new Exception("Destination directory name is empty");
        }
    }
}
