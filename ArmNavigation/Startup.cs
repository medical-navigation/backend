using ArmNavigation.Infrastructure.Postgres.Extensions;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace ArmNavigation;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public void ConfigureServices(IServiceCollection service)
    {
        service.AddCors();
        service.AddSerilog();
        service.AddRouting();

        service.ConfigurePostgresInfrastructure();
    }

    public void Configure(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseRouting();

        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI();
        applicationBuilder.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        applicationBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

    }
}