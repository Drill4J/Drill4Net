using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll;
using Drill4Net.Demo.OnlineStore.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Models.Mapping
{
    public class CartDtoProfile: Profile
    {
        public CartDtoProfile()
        {
            CreateMap<CartDto, Cart>();
        }
    }
}
