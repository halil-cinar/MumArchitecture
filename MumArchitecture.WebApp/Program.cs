using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Routing.Constraints;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Middleware;
using MumArchitecture.Business.Services;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.DataAccess.Repository.EntityFramework;
using MumArchitecture.Domain;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var assemblies = AppDomain.CurrentDomain.GetAssemblies();
foreach (var assembly in assemblies)
{
    builder.Services.AddScopedServices(assembly);
}
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfGenericRepositoryBase<>));
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<DatabaseContext>();
// Add services to the container.
builder.Services.AddControllersWithViews().AddNToastNotifyToastr();
builder.Services.AddMemoryCache();

var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);
appSettings.serviceProvider = builder.Services.BuildServiceProvider();
AppSettings.Initialize(appSettings);

builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


if (appSettings.ApiEnabled)
{
    // Swagger
    builder.Services.AddEndpointsApiExplorer();     // minimal API’ler için gerekli
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "MUM API (+ MVC)",
            Version = "v1"
        });

        // XML yorumlarýný dahil etmek isterseniz:
        var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);


        options.DocInclusionPredicate((_, _) => true);
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}
else
{
    if (appSettings.ApiEnabled)
    {
        app.UseSwagger();                // /swagger/v1/swagger.json üretir
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1 dokümantasyon");
            c.RoutePrefix = "swagger";   // => /swagger (root’a deploy etmek isterseniz "")
            c.ConfigObject.AdditionalItems["displayOperationId"] = false;
        });
    }
}
app.UseMiddleware<CultureMiddleware>();
app.UseMiddleware<RequestLogMiddleware>();
app.UseMiddleware<SecurityMiddleware>();
app.UseMiddleware<SessionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<ValidationMiddleware>();
app.UseMiddleware<CachingMiddleware>();
app.UseAuthorization();
app.UseIpRateLimiting();
app.UseNToastNotify();

if (!appSettings.ApiEnabled)
{
    app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/api"), branch =>
    {
        branch.Run(async ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound; // 403, 410, 501 vb. da verebilirsiniz
            await ctx.Response.WriteAsync("API eriþimi kapalý");
        });
    });
}
else
{
    app.Use(async (context, next) =>
    {
        context.Request.EnableBuffering(); // API için gerekli
        await next.Invoke();
    });
}

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

FirstStart.CreateDB();
app.Run();
