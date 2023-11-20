namespace Authentication;

public class UserAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public UserAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var user = ApplicationContext.CurrentUser;

        if (user != null && user.UserName == "TestUser2")
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Unauthorized User");
            return;
        }
            
        await _next(context);
    }
}