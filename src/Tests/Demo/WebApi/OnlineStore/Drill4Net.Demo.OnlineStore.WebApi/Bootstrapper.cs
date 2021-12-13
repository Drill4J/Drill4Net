using Microsoft.Extensions.DependencyInjection;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Services;
using Drill4Net.Demo.OnlineStore.Dal.Mapping;
using Drill4Net.Demo.OnlineStore.Dal.Services;
using Drill4Net.Demo.OnlineStore.WebApi.Mapping;

namespace Drill4Net.Demo.OnlineStore.WebApi
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddTransientServices( this IServiceCollection services)
        {
            services.AddTransient<IProductDataReadService, ProductInMemoryStorage>();
            services.AddTransient<IProductDataWriteService, ProductInMemoryStorage>();
            services.AddTransient<ICartDataReadService, CartInMemoryStorage>();
            services.AddTransient<ICartDataWriteService, CartInMemoryStorage>();
            services.AddTransient<IProductBusinessService, ProductService>();
            services.AddTransient<ICartBusinessService, CartService>();
            return services;
        }
        public static IServiceCollection AddAutoMapperService(this IServiceCollection services)
        {
            services.AddAutoMapper(
                typeof(ApiDtoProfile).Assembly, 
                typeof(DalProfile).Assembly);
            return services;
        }

        
    }
}
