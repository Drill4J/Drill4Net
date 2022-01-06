using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Configurator
{
    public abstract class AbstractCiEditor : AbstractConfiguratorCommand
    {
        protected AbstractCiEditor(ConfiguratorRepository rep) : base(rep)
        {
        }

        /**************************************************************************/

        public bool Edit(bool isNew)
        {
            throw new System.NotImplementedException();
        }
    }
}
