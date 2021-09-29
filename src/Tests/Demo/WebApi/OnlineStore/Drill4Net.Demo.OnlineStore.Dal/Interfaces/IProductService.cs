using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Interfaces
{
    public interface IProductService: IServiceBase<Product>
    {
        IEnumerable<Product> GetAll();
        void Delete(Guid id);
    }
}
