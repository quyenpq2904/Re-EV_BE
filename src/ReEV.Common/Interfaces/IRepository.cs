namespace ReEV.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<PaginationResult<T>> GetAllAsync(int page = 1, int pageSize = 10, string search = "");

        Task<T?> GetByIdAsync(Guid id);

        Task<T> CreateAsync(T entity);

        Task<T?> UpdateAsync(Guid id, T entity);

        Task<T?> DeleteAsync(Guid id);
    }
}
