using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Authentication;

public class UserIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public UserIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
            var userIdClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == "UserID");

            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userId);

                if (user != null)
                {
                    ApplicationContext.CurrentUser = user;
                }

                //Temporary using in this middleware
                if (user != null)
                {
                    var userRoles = await userManager.GetRolesAsync(user);
                }
            }

            await _next(context);
    }
}