using System.IdentityModel.Tokens.Jwt;
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
            OnMessageReceived = context =>
            {
                if (context.Request.Headers["Authorization"].Count == 0)
                    context.Response.Headers.Add("TokenException", "AuthorizationHeaderException");

                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var iatClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == "iat");
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
    if (context.Response.Headers.ContainsKey("TokenException"))
    {
        var tokenException = context.Response.Headers["TokenException"];
        context.Response.StatusCode = 403;

        if (tokenException == "AuthorizationHeaderException")
            await context.Response.WriteAsync("Authorization header not set");
        else if (tokenException == "IatHasPassedException")
            await context.Response.WriteAsync("Refresh Token");
        else if (tokenException == "SecurityTokenInvalidIssuerException")
            await context.Response.WriteAsync("This token with issuer 'Microsoft' doesn't belong to this domain");
        else if (tokenException == "SecurityTokenExpiredException")
            await context.Response.WriteAsync("Token Expired");
    }
    else
        await next();
});

app.UseMiddleware<UserIdMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();