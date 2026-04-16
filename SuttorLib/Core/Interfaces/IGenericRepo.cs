namespace SuttorLibrary.Core.Interfaces
{
    public interface IGenericRepo<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T?> GetById(string id);
        Task Add(T entity);
        void Update(T entity);
        void Delete(string id);
    }
}
