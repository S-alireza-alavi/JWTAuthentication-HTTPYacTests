namespace Authentication;

public class UnauthorizedUserMiddleware
{
    private readonly RequestDelegate _next;

    public UnauthorizedUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == "UserID");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            var user = AppContext.CurrentUser;

            if (user != null && user.UserName == "TestUser2")
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Unauthorized User");
                return;
            }
        }

        await _next(context);
    }
}