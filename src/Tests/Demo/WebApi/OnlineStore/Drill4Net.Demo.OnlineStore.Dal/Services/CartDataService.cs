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
    public class CartDataService : ICartDataService
    {
        private readonly IMapper _mapper;
        public CartDataService(IMapper mapper)
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

        private Models.Cart GetDalItem(Guid id)
        {
            return DataContext.Carts.FirstOrDefault(x => x.Id == id);
        }
        public Cart Get(Guid id)
        {
            var dalItem= GetDalItem(id);
            return _mapper.Map<Bll.Models.Cart>(dalItem);
        }

        public void Update(Cart item)
        {

        }
    }
}
