using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class ProductDataService : IProductDataService
    {
        private readonly IMapper _mapper;
        public ProductDataService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public Product Create(Product item)
        {
            //item.Id = Guid.NewGuid();
            var dalItem = _mapper.Map<Dal.Models.Product>(item);
            DataContext.Products.Add(dalItem);
            return item;
        }
        public void Delete(Guid id)
        {
            var product = GetDalItem(id);
            if (product != null)
            {
                DataContext.Products.Remove(product);
            }
        }
        public Product Get(Guid id)
        {
            var dalItem= GetDalItem(id);
            return _mapper.Map<Bll.Models.Product>(dalItem);
        }
        private Models.Product GetDalItem(Guid id)
        {

            return DataContext.Products.FirstOrDefault(x => x.Id == id);
        }
        public IEnumerable<Product> GetAll()
        {
             return _mapper.Map<IEnumerable<Bll.Models.Product>>(DataContext.Products);
        }
        public void Update(Product item)
        {
            var product = GetDalItem( item.Id);
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
