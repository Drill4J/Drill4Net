using Drill4Net.Demo.OnlineStore.Bll.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Services;
using Drill4Net.Demo.OnlineStore.Dal.Mapping;
using Drill4Net.Demo.OnlineStore.Dal.Services;
using Drill4Net.Demo.OnlineStore.WebApi.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Drill4Net.Demo.OnlineStore.WebApi
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddTransitentServices(IServiceCollection services)
        {
            services.AddTransient<IProductDataReadService, ProductDataReadService>();
            services.AddTransient<IProductDataWriteService, ProductDataWriteService>();
            services.AddTransient<ICartDataReadService, CartDataReadService>();
            services.AddTransient<ICartDataWriteService, CartDataWriteService>();
            services.AddTransient<ICartDataReadService, CartDataReadService>();
            services.AddTransient<IProductBusinessService, ProductService>();
            return services;
        }
        public static IServiceCollection AddAutoMapperService(IServiceCollection services)
        {
            services.AddAutoMapper(
                typeof(ApiDtoProfile).Assembly, 
                typeof(DalProfile).Assembly);
            return services;
        }

        
    }
}
