using Authentication.Models;

namespace Authentication;

public class ApplicationContext
{
    public static ApplicationUser? CurrentUser { get; set; }

    public static Dictionary<string, IList<string>> UserRoles =
        new Dictionary<string, IList<string>>();
}