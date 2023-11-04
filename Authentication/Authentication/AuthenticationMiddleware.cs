using System.Security.Claims;
using JWT.Builder;
using System.Text.Json;

namespace Authentication;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Authorization header not set");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length);
        var tokenObject = new JwtBuilder()
            .WithValidationParameters(parameters => parameters.ValidateSignature = false)
            .Decode<IDictionary<string, object>>(token);

        var issuer = GetIssuer(tokenObject);

        if (issuer != string.Empty && issuer != "Rayvarz")
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"This token with issuer '{issuer}' doesn't belong to this domain");
            return;
        }

        if (CheckIssuedAt(tokenObject))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Refresh Token");
            return;
        }

        if (CheckExpiration(tokenObject))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Token Expired");
            return;
        }

        if (CheckUserId(tokenObject, out var userId))
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"UserId: {userId}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims, "custom");
            context.User = new ClaimsPrincipal(identity);
            return;
        }

        await _next(context);
    }

    private string GetIssuer(IDictionary<string, object> tokenObject)
    {
        try
        {
            if (tokenObject.TryGetValue("iss", out var issuer))
            {
                return issuer.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    private bool CheckIssuedAt(IDictionary<string, object> tokenObject)
    {
        if (tokenObject.TryGetValue("iat", out var issuedAt) && issuedAt is JsonElement iatElement)
        {
            if (iatElement.ValueKind == JsonValueKind.Number)
            {
                var iat = iatElement.GetInt64();
                var iatDate = DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;
                var validDate = new DateTime(2023, 10, 30);

                return iatDate < validDate;
            }
        }

        return false;
    }

    private bool CheckExpiration(IDictionary<string, object> tokenObject)
    {
        if (tokenObject.TryGetValue("exp", out var expirationTime))
        {
            if (expirationTime.ToString() is string expString)
            {
                var exp = Convert.ToInt64(expString);
                var expTime = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                var today = DateTime.Now;

                return expTime < today;
            }
        }

        return false;
    }
    
    private bool CheckUserId(IDictionary<string, object> tokenObject, out string userId)
    {
        if (tokenObject.TryGetValue("UserId", out var userIdValue))
        {
            userId = userIdValue.ToString();
            return true;
        }
        else
        {
            userId = null;
            return false;
        }
    }
}