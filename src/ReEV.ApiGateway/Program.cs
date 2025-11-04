using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowWebApp", p => p
        .WithOrigins("https://rev.quyenpq.work")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var secret = jwtSection["Secret"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUser", p => p.RequireAuthenticatedUser());
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(async transformContext =>
        {
            var user = transformContext.HttpContext.User;
            var authHeader = transformContext.HttpContext.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                transformContext.ProxyRequest.Headers.Remove("Authorization");
                transformContext.ProxyRequest.Headers.Add("Authorization", authHeader);
            }

            if (user?.Identity?.IsAuthenticated == true)
            {
                var sub = user.FindFirst("sub")?.Value ?? "";
                var email = user.FindFirst("email")?.Value ?? "";
                var role = user.FindFirst("role")?.Value ?? "";
                transformContext.ProxyRequest.Headers.Add("X-User-Id", sub);
                transformContext.ProxyRequest.Headers.Add("X-User-Email", email);
                transformContext.ProxyRequest.Headers.Add("X-User-Role", role);
            }
        });
    });

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok("OK"));

app.MapReverseProxy();

app.Run();