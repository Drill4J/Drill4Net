using System;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;

namespace Drill4Net.Demo.OnlineStore.Bll.Services
{
    public class ProductService : IProductBusinessService
    {
        private readonly IProductDataWriteService _productDataServiceWrite;

        /******************************************************************/

        public ProductService (IProductDataWriteService productDataServiceWrite)
        {
            _productDataServiceWrite = productDataServiceWrite;
        }

        /******************************************************************/

        public Product AddProduct(Product product)
        {
           return _productDataServiceWrite.Create(product);
        }

        public void DeleteProduct(Guid product)
        {
            _productDataServiceWrite.Delete(product);
        }

        public void UpdateProduct(Product product)
        {
            _productDataServiceWrite.Update(product);
        }
    }
}
