﻿using System.Reflection;
using System.Text;
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
            var corsName = EnvironmentManager.APP_CORS_NAME;
            var corsClientAllowedOrigins = EnvironmentManager.APP_CORS_ALLOWED_ORIGINS?.Split(',') ?? Array.Empty<string>();

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
            string secret = EnvironmentManager.APP_SECRET_KEY;
            string issuer = EnvironmentManager.APP_ISSUER;
            string audience = EnvironmentManager.APP_AUDIENCE;

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

                    ValidIssuer = issuer,
                    ValidAudience = audience,
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

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                option.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
        }

        public static void AddConnectionAndHealthChecks(this IServiceCollection services)
        {
            string sql = EnvironmentManager.APP_SQL_AUTHENTICATION;
            services.AddHealthChecks()
                .AddCheck("database", new SQLHealthCheck(sql));

            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(sql));
        }

        public static void AddInterfaceManager(this IServiceCollection services)
        {
            services.AddScoped<RequestContext>();
            services.AddScoped(typeof(IServicesManager<>), typeof(ServicesManager<>));
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }
    }
}
