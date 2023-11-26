using Nest;

namespace DapperWithELK.Services
{
    public class ElasticsearchService<T> : IElasticsearchService<T> where T : class
    {
        private readonly ElasticClient client;
         
        public ElasticsearchService(ElasticClient elastic)
        {
            client = elastic;
        }
        public async Task<string> CreateDocumentAsync(T document)
        {
            var Response=await client.IndexDocumentAsync(document);

            return Response.IsValid ? "Document created successfully":"Failed to create document";
        }

        public async Task<string> DeleteDocumentAsync(int id)
        {
            var Response = await client.DeleteAsync(new DocumentPath<T>(id));

            return Response.IsValid ? "Document delete successfully" : "Failed to delete document";
        }

        public async Task<IEnumerable<T>> GetAllDocuments()
        {
            var searchResponse = await client.SearchAsync<T>(s => s
                                        .MatchAll()
                                        .Size(100));
            return searchResponse.Documents;
        }

        public async Task<T> GetDocumentAsync(int id)
        {
           var Response = await client.GetAsync(new DocumentPath<T>(id));
            return Response.Source;
        }

        public async Task<string> UpdateDocumentAsync(T document)
        {
            var Response = await client.UpdateAsync(new DocumentPath<T>(document), u => u
                                    .Doc(document)
                                    .RetryOnConflict(3));
            return Response.IsValid ? "Document update successfully" : "Failed to update document";

        }
    }
}
