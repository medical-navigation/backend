using ArmNavigation.Infrastructure.Postgres.Extensions;
using ArmNavigation.Infrastructure.Postgres.Repositories;
using ArmNavigation.Services;
using ArnNavigation.Application.Repositories;
using ArnNavigation.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

namespace ArmNavigation;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public void ConfigureServices(IServiceCollection service)
    {
        service.AddCors();
        service.AddSerilog();
        service.AddRouting();
        service.AddControllers();
        service.AddEndpointsApiExplorer();

        // Настройка Swagger с JWT поддержкой
        service.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "ArmNavigation API", Version = "v1" });

            // Добавляем поддержку JWT в Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        });

        // JWT authentication
        var jwtKey = _configuration["Jwt:Key"] ?? "your_super_secret_key_that_is_at_least_32_characters_long!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "ArmNavigation";
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

        service
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };
            });

        service.AddAuthorization();

        // Application services
        service.AddScoped<JwtTokenService>();
        service.AddScoped<IAuthService, JwtAuthService>();
        service.AddScoped<IPasswordHasher, PasswordHasher>();
        service.AddScoped<IUsersService, UsersService>();
        service.AddScoped<ICarsService, CarsService>();
        service.AddScoped<IMedInstitutionService, MedInstitutionService>();

        // Repositories
        service.AddScoped<IUserRepository, UserRepository>();
        service.AddScoped<ICarRepository, CarRepository>();
        service.AddScoped<IMedInstitutionRepository, MedInstitutionRepository>();

        service.ConfigurePostgresInfrastructure();
    }

    public void Configure(IApplicationBuilder applicationBuilder, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            applicationBuilder.UseDeveloperExceptionPage();
        }

        applicationBuilder.UseRouting();

        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI();

        applicationBuilder.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

        applicationBuilder.UseAuthentication();
        applicationBuilder.UseAuthorization();

        applicationBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}