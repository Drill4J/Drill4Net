using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll;
using Drill4Net.Demo.OnlineStore.Bll.Models;
using Drill4Net.Demo.OnlineStore.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.WebApi.Models.Mapping
{
    public class ProductDtoProfile: Profile
    {
        public ProductDtoProfile()
        {
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<NewProductDto, Product>().ReverseMap();
        }
    }
}
