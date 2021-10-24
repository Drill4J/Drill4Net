using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using Drill4Net.Demo.OnlineStore.Dal.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class CartDataReadService : ICartDataReadService
    {
        private readonly IMapper _mapper;
        public CartDataReadService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public Cart GetCart(Guid id)
        {
            var dalItem= CartDataHelper.GetCart(id);
            return _mapper.Map<Bll.Contracts.Models.Cart>(dalItem);
        }
    }
}
