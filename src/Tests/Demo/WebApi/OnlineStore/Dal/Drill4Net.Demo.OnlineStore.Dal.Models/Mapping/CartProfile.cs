using AutoMapper;

namespace Drill4Net.Demo.OnlineStore.Dal.Models.Mapping
{
    public class CartProfile: Profile
    {
        public CartProfile()
        {
            CreateMap<Bll.Models.Cart, Dal.Models.Cart>().ReverseMap();
        }
    }
}
