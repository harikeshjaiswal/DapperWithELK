using System.Data.SqlTypes;

namespace DapperWithELK.Services
{
    public interface IElasticsearchService<T>
    {
        Task<string> CreateDocumentAsync(T document);
        Task<T> GetDocumentAsync(int id);
        Task<IEnumerable<T>> GetAllDocuments();

        Task<string> UpdateDocumentAsync(T document);

        Task<string> DeleteDocumentAsync(int id);

    }
}
