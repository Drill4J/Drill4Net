using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Interfaces
{
    public interface IProductBusinessService
    {
        public IEnumerable<Product> GetSortedProductsByPage();
        public IEnumerable<Product> GetFilteredProducts();
        public void AddProduct(Product item);
        public void UpdateProduct(Product item);
        public void DeleteProduct(Guid product);
    }
}
