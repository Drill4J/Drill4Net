using AutoMapper;

namespace Drill4Net.Demo.OnlineStore.Dal.Models.Mapping
{
    public class ProductProfile: Profile
    {
        public ProductProfile()
        {
            CreateMap<Bll.Models.Product, Dal.Models.Product>().ReverseMap();
        }
    }
}
