using DapperWithELK.Extension;
using DapperWithELK.Repo;

namespace DapperWithELK
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddElasticsearch(builder.Configuration);

            // Add services to the container.
            builder.Services.AddControllers();

            // Retrieve the connection string from the configuration
            var connectionString = builder.Configuration.GetConnectionString("connection");

            // Register the EmployeeRepo with the connection string
            builder.Services.AddSingleton<IEmployeeRepo>(_ => new EmployeeRepo(connectionString));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
