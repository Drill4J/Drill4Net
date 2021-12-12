using System;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface IProductDataWriteService: IDataWriteServiceBase<Product>
    {
        void Delete(Guid id);
        void Update(Product product);
    }
}
