using app.interfaces;
using app.migrator.Contexts;
using app.presentations;
using app.repositories;
using app.services;
using app.shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetEscapades.AspNetCore.SecurityHeaders;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

namespace Web.Host.Extensions
{
    internal static class BuilderService
    {
        public static void AddSwaggerUINonceSupport(this IServiceCollection services)
        {
            services.AddOptions<SwaggerUIOptions>()
                .Configure<IHttpContextAccessor>((swaggerUiOptions, httpContextAccessor) =>
                {
                    var originalIndexStreamFactory = swaggerUiOptions.IndexStream;

                    swaggerUiOptions.IndexStream = () =>
                    {
                        using var originalStream = originalIndexStreamFactory();
                        using var originalStreamReader = new StreamReader(originalStream);
                        var originalIndexHtmlContents = originalStreamReader.ReadToEnd();

                        var requestSpecificNonce = httpContextAccessor.HttpContext?.GetNonce();

                        var nonceEnabledIndexHtmlContents = originalIndexHtmlContents
                            .Replace("<script>", $"<script nonce=\"{requestSpecificNonce}\">", StringComparison.OrdinalIgnoreCase)
                            .Replace("<style>", $"<style nonce=\"{requestSpecificNonce}\">", StringComparison.OrdinalIgnoreCase);

                        return new MemoryStream(Encoding.UTF8.GetBytes(nonceEnabledIndexHtmlContents));
                    };
                });
        }

        public static void AddCorsOrigins(this IServiceCollection services)
        {
            var corsName = EnvironmentManager.CORS_NAME;
            var corsClientAllowedOrigins = EnvironmentManager.CORS_ALLOWED_ORIGINS?.Split(',') ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy(corsName, builder => builder
                    .WithOrigins(corsClientAllowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                );
            });
        }

        public static void AddJwtAuthentication(this IServiceCollection services)
        {
            string secret = SecurityUtils.PublicDecrypt(EnvironmentManager.SECRET_KEY);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = EnvironmentManager.ISSUER,
                    ValidAudience = EnvironmentManager.AUDIENCE,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        public static void AddPresentationSwaggerGen(this IServiceCollection services)
        {
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc(PresentationAssemblyReference.Name, new OpenApiInfo
                {
                    Title = PresentationAssemblyReference.Title,
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

                option.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securityScheme, new string[] { } }
                };

                option.AddSecurityRequirement(securityRequirement);
            });
        }

        public static void AddConnectionAndHealthChecks(this IServiceCollection services)
        {
            string sql = EnvironmentManager.SQL_AUTHENTICATION;

            string decryptConnectionString = SecurityUtils.PublicDecrypt(sql);
            services.AddHealthChecks()
                .AddCheck("database", new SQLHealthCheck(decryptConnectionString));

            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(decryptConnectionString));
        }

        public static void AddInterfaceManager(this IServiceCollection services)
        {
            services.AddScoped<RequestContext>();
            services.AddScoped(typeof(IServicesManager<>), typeof(ServicesManager<>));
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }
    }
}
