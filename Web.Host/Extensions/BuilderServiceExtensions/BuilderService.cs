using api.migrator;
using api.repository;
using api.repository.Authorization;
using api.service;
using api.service.abstractions;
using api.web.host.presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Web.Host.Extensions.BuilderServiceExtensions.BuilderServiceComponent;

namespace Web.Host.Extensions.BuilderServiceExtensions
{
    public static class BuilderService
    {
        public static void AddCorsOrigins(this IServiceCollection services, IConfiguration configuration)
        {
            string[] corsOrigins = configuration.GetSection("App:CorsOrigins").Get<string[]>();
            if (corsOrigins is null)
            {
                string errorMessage = "Missing Cors Configuration";

                Console.WriteLine(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            services.AddCors(options =>
            {
                options.AddPolicy(configuration["App:CorsName"],
                        builder => builder
                        .WithOrigins(corsOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                    );
            });
        }
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            string secretKey = configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                string errorMessage = "Missing Secretkey Configuration";

                Console.WriteLine(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = false,
                     ValidateIssuerSigningKey = true,

                     ValidIssuer = configuration["Jwt:Issuer"],
                     ValidAudience = configuration["Jwt:Audience"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
                 };
             });
        }
        public static void AddPresentationSwaggerGen(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(PresentationAssemblyReference.Version, new OpenApiInfo
                {
                    Title = "WebApp",
                    Version = PresentationAssemblyReference.Version,
                    Description = "Special thanks to all professional developers who love sharing their ideas to improve the world quality of life.",
                    Contact = new OpenApiContact
                    {
                        Name = "Frank Dev",
                        Email = "pbensi.contact@gmail.com"
                    }
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter Token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securityScheme, new string[] { } }
                };

                c.AddSecurityRequirement(securityRequirement);
            });
        }
        public static void AddConnectionAndHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("WindowsAuthentication");

            services.AddHealthChecks()
                    .AddCheck("database", new BuilderServiceDatabaseHealthCheck(connectionString));

            services.AddDbContext<APIDBContext>(options =>  
                options.UseSqlServer(connectionString));
        }
        public static void AddInterfaceManager(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IServicesManager, ServiceManager>();
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped<IPermissionBuilder, PermissionBuilder>();
            services.AddScoped<IPermissionProvider, PermissionProvider>();
        }
    }
}
