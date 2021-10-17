using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Interfaces
{
    public interface IDataReadServiceBase<T> where T : class
    {
        T Get(Guid id);
    }
}
