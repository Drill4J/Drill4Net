using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using Drill4Net.Demo.OnlineStore.WebApi.Models;


namespace Drill4Net.Demo.OnlineStore.WebApi.Mapping
{
    public class ApiDtoProfile: Profile
    {
        public ApiDtoProfile()
        {
            CreateMap<CartDto, Cart>().ReverseMap();
            CreateMap<NewCartDto, Cart>().ReverseMap();
            CreateMap<CartItemDto, CartItem>().ReverseMap();
            CreateMap<ProductDto, CartItem>()
                .ForMember("ProductId", opt => opt.MapFrom(p => p.Id))
                .ForMember("ProductName", opt => opt.MapFrom(p => p.Name))
                .ForMember("ProductPrice", opt => opt.MapFrom(p => p.Price));
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<NewProductDto, Product>().ReverseMap();
        }
    }
}
