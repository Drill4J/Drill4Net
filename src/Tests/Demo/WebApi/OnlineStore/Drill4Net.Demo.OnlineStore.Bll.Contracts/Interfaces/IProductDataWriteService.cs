using Drill4Net.Demo.OnlineStore.Bll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Interfaces
{
    public interface IProductDataWriteService: IDataWriteServiceBase<Product>
    {
        void Delete(Guid id);
    }
}
