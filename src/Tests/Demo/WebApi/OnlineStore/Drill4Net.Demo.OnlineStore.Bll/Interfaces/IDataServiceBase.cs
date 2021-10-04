using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Interfaces
{
    public interface IDataServiceBase<T> where T : class
    {
        //IEnumerable<T> GetAll();
        T Get(Guid id);
        T Create(T item);
        void Update(T item);
        //void Delete(int id);
    }
}
