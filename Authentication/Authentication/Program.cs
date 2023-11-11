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
        
        // JwtBearerEvents jwtBearerEvents = new JwtBearerEvents
        // {
        //     OnAuthenticationFailed = delegate(AuthenticationFailedContext context)
        //     {
        //         Exception contextException = context.Exception;
        //         return Task.CompletedTask;
        //     }
        // };

        JwtBearerEvents jwtBearerEvents = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenInvalidIssuerException)
                {
                    context.Response.StatusCode = 403;
                    context.Response.WriteAsync("This token with issuer 'Microsoft' doesn't belong to this domain");
                }
                
                else if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.StatusCode = 403;
                    // TODO: Fix this later
                    // context.Response.WriteAsync("Token Expired");
                }
                
                // Exception contextException = context.Exception;

                return Task.CompletedTask;
            }
        };

        options.Events = jwtBearerEvents;
    });

var app = builder.Build();

// app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthentication();
app.UseMiddleware<UserIdMiddleware>();

// app.MapGet("/", context =>
// {
//     if (context.User.Identity is { IsAuthenticated: true })
//     {
//         return context.Response.WriteAsync("Hello World!");
//     }
//     
//     return Task.CompletedTask;
// });

app.MapGet("/", () => "Hello World!");

app.Run();

// Save this for further usage
/*
if (context.User.FindFirst("iat") is { Value: var iatClaimValue })
        {
            if (long.TryParse(iatClaimValue, out long iatTimeStamp))
            {
                var iatTime = DateTimeOffset.FromUnixTimeSeconds(iatTimeStamp);

                if (iatTime >= DateTime.Now)
                {
                    return context.Response.WriteAsync("Refresh Token");
                }
            }
        }
*/