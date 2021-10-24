using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface IProductBusinessService
    {
        public Product AddProduct(Product product);
        public void UpdateProduct(Product product);
        public void DeleteProduct(Guid product);
    }
}
