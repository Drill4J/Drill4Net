using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Models;
using Drill4Net.Demo.OnlineStore.Dal.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class ProductDataReadService : IProductDataReadService
    {
        private readonly IMapper _mapper;
        public ProductDataReadService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public Product Get(Guid id)
        {
            var dalItem= ProductDataHelper.GetProduct(id);
            return _mapper.Map<Bll.Models.Product>(dalItem);
        }
        public IEnumerable<Product> GetAll()
        {
             return _mapper.Map<IEnumerable<Bll.Models.Product>>(DataContext.Products);
        }

        public IEnumerable<Product> GetSortedProductsByPage(int page, int pageItemsNumber, string sortField)
        {
            var products = DataContext.Products.Skip((page - 1) * pageItemsNumber).Take(pageItemsNumber);
            switch (sortField.ToLower())
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
            return _mapper.Map<IEnumerable<Bll.Models.Product>>(products);

        }

        public IEnumerable<Product> GetFilteredProducts(string category , string namePart)
        {
            throw new NotImplementedException();
        }

    }
}
