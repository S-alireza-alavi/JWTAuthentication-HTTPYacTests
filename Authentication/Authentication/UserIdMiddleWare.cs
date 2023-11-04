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
        var userIdClaim = context.User.FindFirst(claim => claim.Type == "UserID" && claim.Value == "1001");
            
            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;
                context.Items["UserID"] = userId;
            }

            await _next(context);
    }
}