using System;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface ICartDataReadService: IDataReadServiceBase<Cart>
    {
        public Cart GetCart(Guid id);
    }
}
