using System.Text;
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
            ValidateLifetime = true
        };
    });

var app = builder.Build();

// app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthentication();
app.UseMiddleware<UserIdMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();