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
        .WithOrigins(
            "http://localhost:3000",
            "https://localhost:3000",
            "http://rev.quyenpq.work",
            "https://rev.quyenpq.work"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
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
            var logger = transformContext.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            
            // Log để debug
            logger.LogDebug("Gateway Transform: Path={Path}, IsAuthenticated={IsAuth}, HasAuthHeader={HasAuth}", 
                transformContext.HttpContext.Request.Path,
                user?.Identity?.IsAuthenticated ?? false,
                !string.IsNullOrWhiteSpace(authHeader));

            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                transformContext.ProxyRequest.Headers.Remove("Authorization");
                transformContext.ProxyRequest.Headers.Add("Authorization", authHeader);
            }

            if (user?.Identity?.IsAuthenticated == true)
            {
                // Tìm claim "sub" từ nhiều nguồn khác nhau
                var sub = user.FindFirst("sub")?.Value 
                    ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                    ?? "";
                
                var email = user.FindFirst("email")?.Value 
                    ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value
                    ?? "";
                
                var role = user.FindFirst("role")?.Value 
                    ?? user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                    ?? "";
                
                // Log tất cả claims để debug
                logger.LogDebug("Gateway Transform: All claims: {Claims}", 
                    string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}")));
                
                logger.LogDebug("Gateway Transform: Forwarding headers - UserId={UserId}, Email={Email}, Role={Role}", 
                    sub, email, role);
                
                if (!string.IsNullOrEmpty(sub))
                {
                    transformContext.ProxyRequest.Headers.Add("X-User-Id", sub);
                    transformContext.ProxyRequest.Headers.Add("X-User-Email", email);
                    transformContext.ProxyRequest.Headers.Add("X-User-Role", role);
                }
                else
                {
                    logger.LogError("Gateway Transform: User authenticated but no 'sub' claim found! All claims: {Claims}", 
                        string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}")));
                }
            }
            else
            {
                logger.LogWarning("Gateway Transform: User not authenticated. Path={Path}, AuthHeader={AuthHeader}", 
                    transformContext.HttpContext.Request.Path,
                    string.IsNullOrWhiteSpace(authHeader) ? "missing" : "present");
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