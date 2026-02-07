using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Domain.Models;
using SmartInventoryManagementSystem.Infrastructure.Persistence;
using SmartInventoryManagementSystem.Infrastructure.Repositories;
using SmartInventoryManagementSystem.Infrastructure.Services;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer
(options => options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["AppSettings:Issuer"],
    ValidAudience = builder.Configuration["AppSettings:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!))
});


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddHttpContextAccessor();


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:7039") // Blazor Client URL
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.UseAuthentication();    
app.UseAuthorization();

app.MapControllers();

app.Run();
