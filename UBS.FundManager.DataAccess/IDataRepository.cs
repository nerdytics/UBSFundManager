using System.Collections.Generic;
using System.Threading.Tasks;

namespace UBS.FundManager.DataAccess
{
    /// <summary>
    /// Exposes supported CRUD operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataRepository<T> where T : class
    {
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetAll(int datasetSize);
        Task<T> CreateItemAsync(T item);
        Task DeleteItemAsync(string id);
        Task<T> UpdateDocumentAsync(string id, T item);
    }
}
