using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ReEV.Service.Marketplace.Mappings;
using ReEV.Service.Marketplace.Middleware;
using ReEV.Service.Marketplace.Repositories;
using ReEV.Service.Marketplace.Repositories.Interfaces;
using ReEV.Service.Marketplace.Services;
using ReEV.Service.Marketplace.Services.Interfaces;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Không cần JWT authentication ở service level vì Gateway đã validate và forward user info qua headers
// Service sẽ trust Gateway và đọc user info từ custom headers (X-User-Id, X-User-Email, X-User-Role)
builder.Services.AddAuthentication("GatewayAuth")
    .AddScheme<AuthenticationSchemeOptions, GatewayAuthHandler>("GatewayAuth", null);
builder.Services.AddAuthorization();
builder.Services.AddAutoMapper(config => config.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"], typeof(ListingProfile));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth Service API",
        Version = "v1"
    });
    // nếu có JWT, thêm security definition tương tự
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer <token>'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
          new OpenApiSecurityScheme
          {
              Reference = new OpenApiReference
              {
                  Type = ReferenceType.SecurityScheme,
                  Id   = "Bearer"
              }
          },
          new string[] {}
        }
    });
});

builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IFavouriteService, FavouriteService>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddSingleton<RabbitMQPublisher>();

builder.Services.AddHostedService<UserSyncWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Service API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
