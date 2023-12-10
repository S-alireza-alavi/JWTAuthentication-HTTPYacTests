using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Authentication;

public class UserMiddleware
{
    private readonly RequestDelegate _next;
    
    public UserMiddleware(RequestDelegate next)
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

                    IList<string> userRoles = await userManager.GetRolesAsync(user);

                    ApplicationContext.UserRoles[user.PhoneNumber] = userRoles;
                }
            }

            await _next(context);
    }
}