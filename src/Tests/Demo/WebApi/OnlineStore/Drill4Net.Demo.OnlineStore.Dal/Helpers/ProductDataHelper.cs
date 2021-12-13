using System;
using System.Linq;

namespace Drill4Net.Demo.OnlineStore.Dal.Helpers
{
    internal static class ProductDataHelper
    {
        internal static Models.Product GetProduct(Guid id)
        {

            return DataContext.Products.FirstOrDefault(x => x.Id == id);
        }
    }
}
