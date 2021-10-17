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

        IEnumerable<Product> IProductDataReadService.GetSortedProductsByPage()
        {
            throw new NotImplementedException();
        }

        IEnumerable<Product> IProductDataReadService.GetFilteredProducts()
        {
            throw new NotImplementedException();
        }
    }
}
