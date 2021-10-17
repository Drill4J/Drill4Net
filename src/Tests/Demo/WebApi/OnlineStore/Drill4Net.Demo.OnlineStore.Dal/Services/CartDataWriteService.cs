using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class CartDataWriteService : ICartDataWriteService
    {
        private readonly IMapper _mapper;
        public CartDataWriteService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public Cart Create(Cart item)
        {
            //item.Id = Guid.NewGuid();
            var dalItem= _mapper.Map<Dal.Models.Cart>(item);
            DataContext.Carts.Add(dalItem);
            return item;
        }

        public void Update(Cart item)
        {

        }
    }
}
