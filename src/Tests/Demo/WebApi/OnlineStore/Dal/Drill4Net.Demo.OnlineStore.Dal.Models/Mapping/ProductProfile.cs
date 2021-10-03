using AutoMapper;
using Bll= Drill4Net.Demo.OnlineStore.Bll;
using Dal=Drill4Net.Demo.OnlineStore.Dal.Models;

namespace Drill4Net.Demo.OnlineStore.Models.Mapping
{
    public class ProductProfile: Profile
    {
        public ProductProfile()
        {
            CreateMap<Bll.Product, Dal.Models.Product>();
            CreateMap<Product, ProductDto>();
        }
    }
}
