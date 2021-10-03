using Drill4Net.Demo.OnlineStore.Bll.Models;
using Drill4Net.Demo.OnlineStore.Dal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class ProductService : IProductDataService
    {
        public void Create(Product item)
        {
            item.Id = Guid.NewGuid();
            DataContext.Products.Add(item);
        }
        public void Delete(Guid id)
        {
            var product = Get(id);
            if (product != null)
            {
                DataContext.Products.Remove(product);
            }
        }
        public Product Get(Guid id)
        {
            return DataContext.Products.FirstOrDefault(x => x.Id == id);
        }
        public IEnumerable<Product> GetAll()
        {
            return DataContext.Products;
        }

        public void Update(Product item)
        {
            var product = Get(item.Id);
            if (product != null)
            {
                product.Name = item.Name;
                product.Price = item.Price;
                product.Stock = product.Stock;
            }
        }
    }
}
