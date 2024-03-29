using FluentValidation;
using Ozon.ProductService.Api.GrpcServices;
using Ozon.ProductService.Api.Interceptors;
using Ozon.ProductService.DataAccess;
using Ozon.ProductService.Service;

namespace Ozon.ProductService.Api
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddGrpc(o =>
            {
                o.Interceptors.Add<ValidationInterceptor>();
                o.Interceptors.Add<LoggingInterceptor>();
                o.Interceptors.Add<ExceptionInterceptor>();
            }).AddJsonTranscoding();
            
            builder.Services.AddGrpcSwagger();
            builder.Services.AddSwaggerGen();
            builder.Services.AddValidatorsFromAssemblyContaining(typeof(Startup));
            builder.Services.AddRepositories();
            builder.Services.AddServices();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGrpcService<ProductGrpcService>();

            app.Run();
        }
    }
}