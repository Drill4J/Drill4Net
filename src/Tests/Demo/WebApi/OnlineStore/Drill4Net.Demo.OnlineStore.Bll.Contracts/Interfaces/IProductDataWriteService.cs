using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface IProductDataWriteService: IDataWriteServiceBase<Product>
    {
        void Delete(Guid id);
        void Update(Product product);
    }
}
