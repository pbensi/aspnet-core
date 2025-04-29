using app.migrator.Contexts;
using app.presentations;
using app.services;
using app.services.SignalR;
using app.shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Web.Host;
using Web.Host.Extensions;

Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentManager.APP_HOSTING_ENVIRONMENT);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(option => option.AddServerHeader = false);

var logFilePath = builder.Configuration["Logging:LogEvent:LogFilePath"];
var rootLogFilePath = Path.Combine(builder.Environment.ContentRootPath, logFilePath!);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .WriteTo.Console()
        .WriteTo.File(
            rootLogFilePath,
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: LogEventLevel.Debug,
            retainedFileCountLimit: 2
            )
);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<NonceAttribute>();
}).AddApplicationPart(typeof(PresentationAssemblyReference).Assembly);

builder.Services.AddSwaggerUINonceSupport();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;                          // Prevent client-side access to session cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only send cookies over HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict;           // Mitigate CSRF attacks
    options.IdleTimeout = TimeSpan.FromMinutes(10);          // Set session timeout to 20 minutes of inactivity
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddInterfaceManager();
builder.Services.AddConnectionAndHealthChecks();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddCorsOrigins();
builder.Services.AddJwtAuthentication();
builder.Services.AddPresentationSwaggerGen();
builder.Services.AddSingleton<IUserIdProvider, SignalRProvider>();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseSqlServerHealthCheck();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

//https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders
app.UseSecurityHeaders(new HeaderPolicyCollection()
    .RemoveServerHeader()
    .AddFrameOptionsDeny()
    .AddContentTypeOptionsNoSniff()
    .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365)
    .AddStrictTransportSecurityNoCache()
    .AddReferrerPolicyStrictOriginWhenCrossOrigin()
    .AddXssProtectionBlock()
    .AddContentSecurityPolicy(csp =>
    {
        csp.AddDefaultSrc().Self();
        csp.AddConnectSrc().Self();
        csp.AddScriptSrc().Self().WithNonce();
        csp.AddStyleSrc().Self().WithNonce();
        csp.AddImgSrc().Self().Data();
        csp.AddObjectSrc().None();
        csp.AddFormAction().Self();
        csp.AddFrameAncestors().None();
    })
    .AddCrossOriginEmbedderPolicy(p => p.RequireCorp())
    .AddCrossOriginOpenerPolicy(x => x.SameOrigin())
    .AddCustomHeader("X-Presentation-Version", PresentationAssemblyReference.Version)
);

app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(EnvironmentManager.APP_CORS_NAME);

app.UseAuthentication();
app.UseAuthorization();

app.UseRequestContext();
app.UseRequestThrottling();
app.UseSwaggerAuthentication();
app.UseRolePermission();
app.UseSignalRHub();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/swagger/{PresentationAssemblyReference.Name}/swagger.json", $"{PresentationAssemblyReference.Title}");
    c.EnablePersistAuthorization();
    c.DisplayRequestDuration();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    db.Database.Migrate();
}

app.Run();
