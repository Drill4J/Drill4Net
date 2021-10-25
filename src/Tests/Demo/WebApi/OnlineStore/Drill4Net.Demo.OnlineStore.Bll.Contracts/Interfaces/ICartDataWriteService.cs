using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface ICartDataWriteService: IDataWriteServiceBase<Cart>
    {
        public void AddToCart(Guid cartId, Guid productId, int amount);
        public void ChangeItemAmount(Guid cartId, Guid productId, int amount);
        public void RemoveFromCart(Guid cartId, Guid productId);
    }
}
