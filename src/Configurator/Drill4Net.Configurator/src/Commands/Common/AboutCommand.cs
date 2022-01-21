using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Cli;

namespace Drill4Net.Configurator
{
    [CliCommandAttribute(ConfiguratorConstants.CONTEXT_ABOUT)]
    public class AboutCommand : AbstractConfiguratorCommand
    {
        public AboutCommand(ConfiguratorRepository rep, CliCommandRepository cliRep) : base(rep, cliRep)
        {
        }

        /**************************************************************************/

        public override Task<(bool done, Dictionary<string, object> results)> Process()
        {
            var mess = @"
  1. ABOUT WORKFLOW

Instrumentation is the process of obtaining useful data in runtime from the target application (SUT - system under test), in this case test coverage. It is provided by the preliminary injection of the inspection code in assemblies (files) in the ""offline"", for example, in the pipeline CI/CD. This is done by a special injector that operates not with a high-level language like C#, but with an intermediate bytecode - CIL (Common Intermediate Language). Only after that, the modified assemblies are run in the usual way, for example, on a test server.
";
            RaiseMessage(mess, CliMessageType.Help);

            mess = @"    To configure this, use the ""trg"" command group, for example, ""trg new"".";
            RaiseMessage(mess, CliMessageType.Info);

            mess = @"
If the target has automated tests, you should use the Test Runner program, which will determine which tests should be run due to the changed source code, and will run only them - with the necessary options. This achieves significant time savings in software development and testing.
";
            RaiseMessage(mess, CliMessageType.Help);

            mess = @"    To configure this, use the ""run"" command group, for example, ""run new"".";
            RaiseMessage(mess, CliMessageType.Info);

            mess = @"
The injector can handle several different .NET builds, and Test Runner is able to run tests from different projects at the same time.All these chains, both interconnected and independent, can be run in a single CI command, which will automatically be executed under any conditions in some environment. For example, its call can be inserted into the post - build event of compiling the source codes of .NET projects.
";
            RaiseMessage(mess, CliMessageType.Help);

            mess = @"    To configure this, use the ""ci"" command group, for example, ""ci new"". To manually start everything specified in the pipeline – ""ci start"" command.
";
            RaiseMessage(mess, CliMessageType.Info);

            mess = @"For more information, read the information on the project website and help articles for each command separately using ""?"" command.";
            RaiseMessage(mess, CliMessageType.Help);

            mess = @"
  2. ABOUT CLI

The command line interface (CLI) of this program is used both to facilitate the configuration of the CI pipeline through wizards (injecting targets, running automatic tests, implementing the CI startup into the compilation of projects .NET, etc.), and provides manual launch of all operations described in the configs with a command or in the program launch arguments for some ones (""ci start"") - this is used for run CI pipeline.

In turn, the CLI arguments support both Windows syntax and Unix syntax. It is preferable to use the latter. Also, the word order in the command is not important. So, it is allowed to write both ""trg new"" and ""new trg"". The command options should come last, of course.";
            RaiseMessage(mess, CliMessageType.Help);

            mess = @"
    Example: trg edit -- cfg2
    Example: edit trg -- cfg2
";
            RaiseMessage(mess, CliMessageType.Info);

            mess = @"At the same time, for the parameters of the ""?"" (help) special command, you can omit the separator for the positional parameters --.";
            RaiseMessage(mess, CliMessageType.Help);

            mess = @"
    Example: ? -- ci view
    Example: ? ci view
";
            RaiseMessage(mess, CliMessageType.Info);

            mess = "In addition to the usual CLI capabilities, spaces are allowed in named options around the = sign.";
            RaiseMessage(mess, CliMessageType.Help);

            mess = @"
    Example: ci edit --{CoreConstants.ARGUMENT_CONFIG_PATH}=""d:\ci_1.yml""
    Example: ci edit --{CoreConstants.ARGUMENT_CONFIG_PATH} = ""d:\ci_1.yml""
";
            RaiseMessage(mess, CliMessageType.Info);

            mess = "Warning: All file and directory paths must be in quotation marks.";
            RaiseMessage(mess, CliMessageType.Help);

            return Task.FromResult(TrueEmptyResult);
        }

        public override string GetShortDescription()
        {
            return "About the Program, its command line interface, main workflow, etc.";
        }

        public override string GetHelp()
        {
            return ""; //nothing
        }
    }
}
