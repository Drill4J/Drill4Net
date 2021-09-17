using System.Reflection;
using Drill4Net.Injector.Core;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyInformationalVersion(
  ThisAssembly.Git.SemVer.Major + "." +
  ThisAssembly.Git.SemVer.Minor + "." +
  ThisAssembly.Git.SemVer.Patch + "-" +
  ThisAssembly.Git.Branch + "+" +
  ThisAssembly.Git.Commit)]

namespace Drill4Net.Injector.Strategies.Blocks
{
    /// <summary>
    /// Strategy for target's injection with classics Flow cross-points
    /// </summary>
    /// <seealso cref="Drill4Net.Injector.Core.CodeHandlerStrategy" />
    public class BlockStrategy: CodeHandlerStrategy
    {
        public BlockStrategy(InjectorOptions opts)
        {
            var helper = new BlockProbeHelper(opts);
            var probeOpts = opts.Probes;

            //after if/else/switch instructions
            if (probeOpts?.SkipIfElseType != true)
                AddHandler(new IfElseHandler(helper));

            //prior if/else operators and br + br.s instructions
            AddHandler(new BranchHandler(helper));

            //for/foreach/do/while cycles
            AddHandler(new CycleHandler(helper));

            //catch, throw
            AddHandler(new ThrowHandler(helper));
            AddHandler(new CatchFilterHandler(helper));
            
            //enter/return
            AddHandler(new ReturnHandler(helper));
            if(probeOpts?.SkipEnterType != true)
                AddHandler(new EnterHandler(helper));
            
            //methods' calls (must be prior AnchorHandler)
            AddHandler(new CallHandler(helper));

            //jump targets + inner calls of the compiler generated members
            AddHandler(new AnchorHandler(helper));
        }
    }
}
