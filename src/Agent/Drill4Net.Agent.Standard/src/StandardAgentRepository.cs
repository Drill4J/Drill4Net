using System.Collections.Generic;
using System.IO;
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
            var converter = new TreeConverter();
            return converter.ToAstEntities(_tree);
        }
    }
}
