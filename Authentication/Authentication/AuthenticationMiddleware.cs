using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
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

        var issuer = GetIssuerFromToken(tokenObject);

        if (issuer != string.Empty && issuer != "Rayvarz")
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync($"This token with issuer '{issuer}' doesn't belong to this domain");
            return;
        }

        if (CheckIfIatIsLowerThanNow(tokenObject))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Refresh Token");
            return;
        }

        if (CheckIfExpHasPassed(tokenObject))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Token Expired");
            return;
        }

        await _next(context);
    }

    private string GetIssuerFromToken(IDictionary<string, object> tokenObject)
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

    private bool CheckIfIatIsLowerThanNow(IDictionary<string, object> tokenObject)
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

    private bool CheckIfExpHasPassed(IDictionary<string, object> tokenObject)
    {
        if (tokenObject.TryGetValue("exp", out var expirationTime))
        {
            if (expirationTime.ToString() is string expString)
            {
                if (long.TryParse(expString, out var exp))
                {
                    var expTime = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                    var today = DateTime.Now;

                    return expTime < today;
                }
                else
                {
                    Console.WriteLine("Failed to parse 'exp' as long: " + expString);
                }
            }
            else
            {
                Console.WriteLine("'exp' is not a string: " + expirationTime.ToString());
            }
        }
        else
        {
            Console.WriteLine("Token does not contain 'exp' claim.");
        }

        return false;
    }
}