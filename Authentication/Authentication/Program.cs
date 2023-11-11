using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
    if (context.Response.Headers.ContainsKey("TokenException"))
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(context.Response.Headers["TokenException"]);
    }
    else
    {
        await next();
    }
});
app.UseMiddleware<UserIdMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();