using System.Security.Claims;

namespace Authentication;

public class UserIdMiddleware
{
    private readonly RequestDelegate _next;

    public UserIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == "UserID");

        if (userIdClaim != null)
        {
            var userId = userIdClaim.Value;
            context.Items["UserID"] = userId;
        }

        await _next(context);
    }
}