using Authentication.Models;

namespace Authentication;

public class ApplicationContext
{
    public static ApplicationUser? CurrentUser { get; set; }
    public static IList<string>? CurrentUserRoles { get; set; }
}