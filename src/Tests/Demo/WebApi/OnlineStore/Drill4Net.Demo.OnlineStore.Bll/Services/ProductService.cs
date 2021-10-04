using Drill4Net.Demo.OnlineStore.Bll.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Services
{
    public class ProductService : IProductBusinessService
    {
        private readonly IProductDataService _productDataService;
        public ProductService (IProductDataService productDataService)
        {
            _productDataService = productDataService;
        }
        public Product AddProduct(Product item)
        {
           return _productDataService.Create(item);
        }

        public void DeleteProduct(Guid product)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetFilteredProducts()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetSortedProductsByPage()
        {
            throw new NotImplementedException();
        }

        public void UpdateProduct(Product item)
        {
            throw new NotImplementedException();
        }
    }
}
