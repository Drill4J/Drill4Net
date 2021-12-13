namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface IDataWriteServiceBase<T> where T : class
    {
        T Create(T item);
    }
}
