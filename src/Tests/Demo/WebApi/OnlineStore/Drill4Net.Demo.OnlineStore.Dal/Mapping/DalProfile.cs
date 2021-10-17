using AutoMapper;

namespace Drill4Net.Demo.OnlineStore.Dal.Mapping
{
    public class DalProfile: Profile
    {
        public DalProfile()
        {
            CreateMap<Bll.Models.Cart, Dal.Models.Cart>().ReverseMap();
            CreateMap<Bll.Models.Product, Dal.Models.Product>().ReverseMap();
        }
    }
}
