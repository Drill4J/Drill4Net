using Drill4Net.Demo.OnlineStore.Bll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Interfaces
{
    public interface IProductDataService: IServiceDataBase<Product>
    {
        IEnumerable<Product> GetAll();
        void Delete(Guid id);
    }
}
