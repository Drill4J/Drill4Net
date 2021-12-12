using System;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface ICartDataWriteService: IDataWriteServiceBase<Cart>
    {
        public void AddToCart(Guid cartId, Guid productId, int amount);
        public void ChangeItemAmount(Guid cartId, Guid productId, int amount);
        public void RemoveFromCart(Guid cartId, Guid productId);
    }
}
