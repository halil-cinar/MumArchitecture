using AspNetCoreRateLimit;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Middleware;
using MumArchitecture.Business.Services;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.DataAccess.Repository.EntityFramework;
using MumArchitecture.Domain;

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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseMiddleware<CultureMiddleware>();
app.UseMiddleware<RequestLogMiddleware>();
app.UseMiddleware<SecurityMiddleware>();
//app.UseMiddleware<SessionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<ValidationMiddleware>();
app.UseMiddleware<CachingMiddleware>();
app.UseAuthorization();
app.UseIpRateLimiting();
app.UseNToastNotify();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

FirstStart.CreateDB();
app.Run();
