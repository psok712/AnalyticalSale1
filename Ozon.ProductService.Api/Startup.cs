using FluentValidation;
using Ozon.ProductService.Api.GrpcServices;
using Ozon.ProductService.Api.Interceptors;
using Ozon.ProductService.DataAccess;
using Ozon.ProductService.Service;

namespace Ozon.ProductService.Api;

public class Startup
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc(o =>
        {
            o.Interceptors.Add<ValidationInterceptor>();
            o.Interceptors.Add<LoggingInterceptor>();
            o.Interceptors.Add<ExceptionInterceptor>();
        });

        services.AddGrpcSwagger();
        services.AddSwaggerGen();
        services.AddValidatorsFromAssemblyContaining(typeof(Startup));
        services.AddRepositories();
        services.AddServices();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapGrpcService<ProductGrpcService>(); });
    }
}