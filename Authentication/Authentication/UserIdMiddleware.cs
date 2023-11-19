using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Authentication;

public class UserIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public UserIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ApplicationDbContext dbContext)
    {
            var userIdClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == "UserID");

            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userId);

                if (user != null)
                {
                    var userJson = JsonSerializer.Serialize(user);

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(userJson);
                    return;
                }
            }

            await _next(context);
    }
}