using ASP_202.Services;
using ASP_202.Data;
using ASP_202.Services.Hash;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using ASP_202.Services.Random;
using ASP_202.Services.Kdf;
using ASP_202.Middleware;
using ASP_202.Services.Validation;
using ASP_202.Services.Email;
using ASP_202.Services.Display;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TimeService>();
builder.Services.AddTransient<DateService>();
builder.Services.AddScoped<DtService>();

builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<IRandomService, RandomServiceV1>();
builder.Services.AddSingleton<IKdfService, HashKdfService>();
builder.Services.AddSingleton<IValidationService, ValidationServiceV1>();
builder.Services.AddSingleton<IEmailService, GmailService>();
builder.Services.AddSingleton<IDisplayService, DisplayServiceUkr>();





//builder.Services.AddDbContext<DataContext>(options =>
//    options.UseMySqlServers(
//        builder.Configuration.GetConnectionString("ASP_202")
//       )
//    );





String? connectionString = builder.Configuration.GetConnectionString("MainDb");

MySqlConnection connection = new MySqlConnection(connectionString);

builder.Services.AddDbContext<DataContext>(options =>
 options.UseMySql(connection, ServerVersion.AutoDetect(connection)));
// new MySqlServerVersion(new Version(8, 0, 23));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Конфігурація НТТР-сесій
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(180);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

// подключаем собственные Middleware
app.UseMiddleware<SessionAuthMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
