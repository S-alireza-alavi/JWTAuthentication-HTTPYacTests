using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Authentication;
using Authentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // options.TokenValidationParameters = new TokenValidationParameters
        // {
        //     ValidateIssuer = true,
        //     ValidIssuer = "Rayvarz",
        //     ValidateAudience = false,
        //     RequireExpirationTime = false,
        //     ValidateLifetime = true,
        //     ValidateIssuerSigningKey = true,
        //     IssuerSigningKey = new X509SecurityKey(new X509Certificate2(@"..\..\Auth.pfx"))
        // };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://zdgocunjwgxchxbxhytu.supabase.co/auth/v1",
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Wjt53JJKMvtjNuQQ3zJ369/1H/BgPnRgxQQMXbqK4a3vaiPPC5n9bV9MJB78Spxc5DxKPtmFt9x0ENJuHjr2lQ=="))
        };

        JwtBearerEvents jwtBearerEvents = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Headers["Authorization"].Count == 0)
                    context.Response.Headers.Add("TokenException", "AuthorizationHeaderException");
            
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var iatClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == "iat");
                
                if (iatClaim != null && long.TryParse(iatClaim.Value, out long iatTimeStamp))
                {
                    var iatTime = DateTimeOffset.FromUnixTimeSeconds(iatTimeStamp);
                    
                    if (iatTime <= new DateTime(2023, 11, 01) || File.Exists("./RefreshToken.txt"))
                        context.Response.Headers.Add("TokenException", "IatHasPassedException");
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                context.Response.Headers.Add("TokenException", $"{context.Exception.GetType().Name}");
            
                return Task.CompletedTask;
            }
        };

        options.Events = jwtBearerEvents;
    });

var app = builder.Build();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseMiddleware<TokenAuthorizationMiddleware>();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.Response.Headers.TryGetValue("TokenException", out var header))
    {
        // context.Response.StatusCode = 403;

        if (header == "AuthorizationHeaderException")
            await context.Response.WriteAsync("Authorization header not set");
        else if (header == "IatHasPassedException")
        {
            // await context.Response.WriteAsync("Refresh Token");
            await next();
        }
        else if (header == "SecurityTokenInvalidIssuerException")
            await context.Response.WriteAsync("This token with issuer 'Microsoft' doesn't belong to this domain");
        else if (header == "SecurityTokenExpiredException")
            await context.Response.WriteAsync("Token Expired");
        else if (header == "SecurityTokenSignatureKeyNotFoundException")
            await context.Response.WriteAsync("Signature key not found");
    }
    else
        await next();
});

app.UseMiddleware<UserMiddleware>();
app.UseMiddleware<UserAuthorizationMiddleware>();

app.MapGet("/GetCurrentUser", async (HttpContext context) =>
{
    if (ApplicationContext.CurrentUser != null)
    {
        await context.Response.WriteAsync(ApplicationContext.CurrentUser.UserName);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("User not found");
    }
});

app.MapGet("/GetRoles",
    async (HttpContext context) =>
    {
        if (ApplicationContext.CurrentUser != null)
        {
            var roles = ApplicationContext.UserRoles[ApplicationContext.CurrentUser.PhoneNumber];

            return roles;
        }

        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("User is null");

        return null;
    });

app.MapGet("/Auth", async (HttpContext context) =>
{
    var refreshToken = false;

    if (context.Response.Headers.TryGetValue("TokenException", out var headerException))
    {
        if (headerException == "IatHasPassedException")
        {
            refreshToken = true;
        }
    }

    if (refreshToken)
    {
        context.Response.Redirect($"http://localhost:5000?refreshToken={refreshToken}");
    }
    else
    {
        await context.Response.WriteAsync("User Authenticated");
    }
});

app.MapGet("/Products", () =>
{
    var products = new[]
    {
        new { Id = 1, Name = "Product 1", Price = 10.99 },
        new { Id = 2, Name = "Product 2", Price = 19.99 },
        new { Id = 3, Name = "Product 3", Price = 24.99 }
    };

    return Results.Ok(products);
});

app.Run();

public partial class Program{}