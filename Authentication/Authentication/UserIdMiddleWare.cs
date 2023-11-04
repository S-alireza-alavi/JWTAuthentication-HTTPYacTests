using System.Security.Claims;

namespace Authentication;

public class UserIdMiddleWare
{
    private readonly RequestDelegate _next;

    public UserIdMiddleWare(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;
                
                context.Items["UserID"] = userId;
            }
        }

        await _next(context);
    }
}