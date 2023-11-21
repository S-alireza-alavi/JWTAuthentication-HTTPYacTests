using Authentication.Models;

namespace Authentication;

public class ApplicationContext
{
    public static ApplicationUser? CurrentUser { get; set; }

    public static Dictionary<ApplicationUser, IList<string>> UserRoles { get; set; } =
        new Dictionary<ApplicationUser, IList<string>>();
}