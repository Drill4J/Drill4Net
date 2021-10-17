using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Models;
using Drill4Net.Demo.OnlineStore.Dal.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class ProductDataWriteService : IProductDataWriteService
    {
        private readonly IMapper _mapper;
        public ProductDataWriteService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public Product Create(Product item)
        {
            var dalItem = _mapper.Map<Dal.Models.Product>(item);
            DataContext.Products.Add(dalItem);
            return item;
        }
        public void Delete(Guid id)
        {
            var product = ProductDataHelper.GetProduct(id);
            if (product != null)
            {
                DataContext.Products.Remove(product);
            }
        }
        public IEnumerable<Product> GetAll()
        {
             return _mapper.Map<IEnumerable<Bll.Models.Product>>(DataContext.Products);
        }
        public void Update(Product item)
        {
            var product = ProductDataHelper.GetProduct( item.Id);
            if (product != null)
            {
                product.Name = item.Name;
                product.Category = item.Category;
                product.Price = item.Price;
                product.Stock = product.Stock;
            }
        }
    }
}
