using api.service;
using api.web.host.presentation;
using Web.Host.Extensions.ApplicationBuilderExtensions;
using Web.Host.Extensions.BuilderServiceExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(PresentationAssemblyReference).Assembly);

builder.Services.AddSession();
builder.Services.AddSignalR();
builder.Services.AddConnectionAndHealthChecks(builder.Configuration);
builder.Services.AddInterfaceManager(builder.Configuration);
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddCorsOrigins(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddPresentationSwaggerGen(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHealthCheckApplication();
app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(builder.Configuration["App:CorsName"]);

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerAuthentication();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/swagger/{PresentationAssemblyReference.Version}/swagger.json", "WebApp");
    c.EnablePersistAuthorization();
    c.DisplayRequestDuration();
});

app.UseErrorHandling();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();