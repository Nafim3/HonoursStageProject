
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//    // Seed users if none exist
//    if (!context.Users.Any())
//    {
//        context.Users.AddRange(
//            new User { Username = "Kashem", Email = "Ks@example.com", PasswordHash = "hashed1", RefreshToken = "rt1", RefreshTokenExpiryDate = DateTime.Now.AddDays(2) },
//            new User { Username = "Mansur", Email = "Mr@example.com", PasswordHash = "hashed2", RefreshToken = "rt2", RefreshTokenExpiryDate = DateTime.Now.AddDays(2) }
//        );
//    }

//    context.SaveChanges();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
