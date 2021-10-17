using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Models;
using Drill4Net.Demo.OnlineStore.WebApi.Models;


namespace Drill4Net.Demo.OnlineStore.WebApi.Mapping
{
    public class ApiDtoProfile: Profile
    {
        public ApiDtoProfile()
        {
            CreateMap<CartDto, Cart>().ReverseMap();
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<NewProductDto, Product>().ReverseMap();
        }
    }
}
