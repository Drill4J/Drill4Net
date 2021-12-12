using System;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using Drill4Net.Demo.OnlineStore.Dal.Helpers;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class ProductInMemoryStorage : IProductDataReadService, IProductDataWriteService
    {
        private readonly IMapper _mapper;

        /******************************************************************/

        public ProductInMemoryStorage(IMapper mapper)
        {
            _mapper = mapper;
        }

        /******************************************************************/

        public IEnumerable<Product> GetSortedProductsByPage(int page, int pageItemsNumber, string sortField)
        {
            var products = DataContext.Products.Skip((page - 1) * pageItemsNumber).Take(pageItemsNumber);
            switch (sortField?.ToLower())
            {
                case "name":
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
                case "price":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "stock":
                    products = products.OrderBy(p => p.Stock).ToList();
                    break;
                default:
                    goto case "name";
            }
            return _mapper.Map<IEnumerable<Bll.Contracts.Models.Product>>(products);
        }

        public IEnumerable<Product> GetFilteredProducts(string category , string namePart)
        {
            var products = DataContext.Products.Where(p => (String.IsNullOrEmpty(category) || p.Category.ToLower() == category.ToLower()) &&
                (String.IsNullOrEmpty(namePart)||p.Name.ToLower().Contains(namePart.ToLower())));
            return _mapper.Map<IEnumerable<Bll.Contracts.Models.Product>>(products);
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

        public void Update(Product item)
        {
            var product = ProductDataHelper.GetProduct(item.Id);
            if (product != null)
            {
                product.Name = item.Name;
                product.Category = item.Category;
                product.Price = item.Price;
                product.Stock = item.Stock;
            }
        }

        public Product GetProduct(Guid id)
        {
            var product = ProductDataHelper.GetProduct(id);
            return _mapper.Map<Bll.Contracts.Models.Product>(product);
        }
    }
}
