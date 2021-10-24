using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface IProductDataReadService: IDataReadServiceBase<Product>
    {
        public IEnumerable<Product> GetSortedProductsByPage(int page, int pageItemsNumber, string sortField);
        public IEnumerable<Product> GetFilteredProducts(string category, string namePart);
    }
}
