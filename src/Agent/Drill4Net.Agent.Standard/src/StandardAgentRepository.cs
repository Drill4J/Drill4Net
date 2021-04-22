using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Drill4Net.Agent.Abstract;
using Drill4Net.Agent.Transport;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Agent.Standard
{
    public class StandardAgentRepository : IAgentRepository
    {
        private readonly InjectedSolution _tree;

        /**************************************************************************************/

        public StandardAgentRepository()
        {
            //Injector rep
            var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var cfg_path = Path.Combine(dirName, CoreConstants.CONFIG_STD_NAME);
            var injRep = new InjectorRepository(cfg_path);

            //tree info
            //TODO: filter by framework version !!!
            _tree = injRep.ReadInjectedTree();
        }

        /**************************************************************************************/

        public ICommunicator GetCommunicator()
        {
            return new Communicator();
        }

        public Dictionary<string, InjectedMethod> MapPointToMethods()
        {
            return _tree.MapPointToMethods();
        }

        public List<AstEntity> GetEntities()
        {
            IEnumerable<InjectedType> injTypes = null;

            // check for different compiling target version 
            //we need only one for current runtime
            var rootDirs = _tree.GetDirectories();
            if (rootDirs.Count() > 1)
            {
                var asmNameByDirs = (from dir in rootDirs
                                     select dir.GetAssemblies()
                                               .Select(a => a.Name)
                                               .Where(a => a.EndsWith(".dll"))
                                               .ToList())
                                     .ToList();
                if (asmNameByDirs[0].Count > 0)
                {
                    var multi = true;
                    for (var i = 1; i < asmNameByDirs.Count; i++)
                    {
                        var prev = asmNameByDirs[i - 1];
                        var cur = asmNameByDirs[i];
                        if (prev.Count != cur.Count || prev.Intersect(cur).Count() == 0)
                        {
                            multi = false;
                            break;
                        }
                    }
                    if (multi) //here many copies of target for diferent runtimes
                    {
                        var execVer = CommonUtils.GetEntryTargetVersioning();
                        InjectedDirectory targetDir = null;
                        foreach (var dir in rootDirs)
                        {
                            var asms = dir.GetAssemblies().ToList();
                            if (asms[0].Version.Version != execVer.Version)
                                continue;
                            targetDir = dir;
                            break;
                        }
                        injTypes = targetDir.GetAssemblies().SelectMany(a => a.GetAllTypes());
                    }
                }
            }
            else
            {
                injTypes = _tree.GetAllTypes();
            }
            injTypes = injTypes.Where(a => !a.IsCompilerGenerated);
            //
            var converter = new TreeConverter();
            return converter.ToAstEntities(injTypes);
        }
    }
}
