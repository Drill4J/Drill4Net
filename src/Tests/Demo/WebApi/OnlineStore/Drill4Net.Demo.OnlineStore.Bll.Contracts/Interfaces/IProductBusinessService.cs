using System;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface IProductBusinessService
    {
        public Product AddProduct(Product product);
        public void UpdateProduct(Product product);
        public void DeleteProduct(Guid product);
    }
}
