using System.IdentityModel.Tokens.Jwt;
using Authentication;
using Authentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Rayvarz",
            ValidateAudience = false,
            RequireExpirationTime = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,
            SignatureValidator = (token, parameters) =>
            {
                SecurityToken jwtSecurityToken = new JwtSecurityToken(token);
                return jwtSecurityToken;
            }
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
                    
                    if (iatTime <= new DateTime(2023, 11, 01))
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

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.Response.Headers.TryGetValue("TokenException", out var header))
    {
        context.Response.StatusCode = 403;

        if (header == "AuthorizationHeaderException")
            await context.Response.WriteAsync("Authorization header not set");
        else if (header == "IatHasPassedException")
            await context.Response.WriteAsync("Refresh Token");
        else if (header == "SecurityTokenInvalidIssuerException")
            await context.Response.WriteAsync("This token with issuer 'Microsoft' doesn't belong to this domain");
        else if (header == "SecurityTokenExpiredException")
            await context.Response.WriteAsync("Token Expired");
    }
    else
        await next();
});

app.UseMiddleware<UserIdMiddleware>();
app.UseMiddleware<UserAuthorizationMiddleware>();

app.MapGet("/", () => "Hello World!");

app.MapGet("/GetCurrentUser", async (HttpContext context, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) =>
{
    var userIdClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == "UserID");

    if (userIdClaim != null)
    {
        var userId = userIdClaim.Value;

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userId);

        if (user != null)
        {
            ApplicationContext.CurrentUser = user;

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(user.UserName);
            return;
        }
    }

    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("User not found");
});

app.MapGet("/GetRoles",
    async (HttpContext context, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) =>
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == "UserID");

        if (userIdClaim != null)
        {
            var userId = userIdClaim.Value;

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userId);

            if (user != null)
            {
                var userRoles = await userManager.GetRolesAsync(user);

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(string.Join(", ", userRoles));
                return;
            }
        }

        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("User not found");
    });

app.Run();

public partial class Program{}