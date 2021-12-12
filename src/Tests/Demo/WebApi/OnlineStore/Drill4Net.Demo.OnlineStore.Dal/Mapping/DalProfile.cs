using AutoMapper;

namespace Drill4Net.Demo.OnlineStore.Dal.Mapping
{
    public class DalProfile: Profile
    {
        public DalProfile()
        {
            CreateMap<Bll.Contracts.Models.Cart, Dal.Models.Cart>().ReverseMap();
            CreateMap<Bll.Contracts.Models.CartItem, Dal.Models.CartItem>().ReverseMap();
            CreateMap<Bll.Contracts.Models.Product, Dal.Models.Product>().ReverseMap();
        }
    }
}
