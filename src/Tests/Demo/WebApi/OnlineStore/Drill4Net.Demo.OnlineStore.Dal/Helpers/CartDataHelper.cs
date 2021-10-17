using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Helpers
{
    internal static class CartDataHelper
    {
        internal static Models.Cart GetCart(Guid id)
        {
            return DataContext.Carts.FirstOrDefault(x => x.Id == id);
        }
    }
}
