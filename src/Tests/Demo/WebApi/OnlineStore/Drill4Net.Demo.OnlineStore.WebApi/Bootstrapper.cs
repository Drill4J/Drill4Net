using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;
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
        public static IServiceCollection AddTransientServices( this IServiceCollection services)
        {
            services.AddTransient<IProductDataReadService, ProductInMemoryStorage>();
            services.AddTransient<IProductDataWriteService, ProductDataWriteService>();
            services.AddTransient<ICartDataReadService, CartDataReadService>();
            services.AddTransient<ICartDataWriteService, CartDataWriteService>();
            services.AddTransient<ICartDataReadService, CartDataReadService>();
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
