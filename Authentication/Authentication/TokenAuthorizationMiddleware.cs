namespace Authentication;

public class TokenAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            if (context.Request.Cookies.TryGetValue("Token", out var token))
            {
                context.Request.Headers["Authorization"] = "Bearer " + token;
            }
        }

        await _next(context);
    }
}