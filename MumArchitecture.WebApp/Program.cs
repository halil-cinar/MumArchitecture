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
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);
appSettings.serviceProvider = builder.Services.BuildServiceProvider();
AppSettings.Initialize(appSettings);
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
app.UseMiddleware<SessionMiddleware>();
app.UseMiddleware<ValidationMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<CachingMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

FirstStart.CreateDB();
app.Run();
