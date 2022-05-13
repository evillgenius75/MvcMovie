using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;

var builder = WebApplication.CreateBuilder(args);

string connectionString = 
    builder.Configuration.GetConnectionString("MvcMovieContext");

if (builder.Environment.IsDevelopment())
    builder.Services.AddDbContext<MvcMovieContext>(options =>
        options.UseSqlite(connectionString));
else 
    builder.Services.AddDbContext<MvcMovieContext>(options =>
        options.UseNpgsql(connectionString));

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
