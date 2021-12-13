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
            CreateMap<ProductInfoDto, CartItem>()
                .ForMember(_ => _.ProductId, opt => opt.MapFrom(p => p.Id))
                .ForMember(_ => _.ProductName, opt => opt.MapFrom(p => p.Name))
                .ForMember(_ => _.ProductPrice, opt => opt.MapFrom(p => p.Price));
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<Product, ProductInfoDto>();
            CreateMap<NewProductDto, Product>();
        }
    }
}
