using DapperWithELK.Model;
using DapperWithELK.Services;
using Nest;

namespace DapperWithELK.Extension
{
    public static class ElasticSearchExtensions
    {
        public static void AddElasticsearch(
            this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["ElasticSettings:baseUrl"];
            var defaultIndex = configuration["ElasticSettings:defaultIndex"];

            var settings = new ConnectionSettings(new Uri(url)).BasicAuthentication("elastic", "JbNb_unwrJy3W0OaZ07n")
                .PrettyJson()
                .DefaultIndex(defaultIndex);


            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticsearchService<tbl_employee>, ElasticsearchService<tbl_employee>>();
            services.AddSingleton(client);
            CreateIndex(client, defaultIndex);
        }


        private static void CreateIndex(IElasticClient client, string indexName)
        {
            if (!client.Indices.Exists(indexName).Exists)
            {
                var createIndexResponse = client.Indices.Create(indexName,
                               index => index.Map<tbl_employee>(x => x.AutoMap())
                           );
            }
           
        }
    }
}
