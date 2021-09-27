using AutoMapper;
using Drill4Net.Demo.OnlineStore.Dal;
using Drill4Net.Demo.OnlineStore.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.WebApi.Mapping
{
    public class ProductDtoMapper
    {
        public ProductDtoMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<ProductDto, Product>());
            var mapper = new Mapper(config);
        }
    }
}
