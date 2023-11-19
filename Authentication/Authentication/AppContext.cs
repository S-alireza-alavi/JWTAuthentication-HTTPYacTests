using Authentication.Models;

namespace Authentication;

public static class AppContext
{
    public static ApplicationUser? CurrentUser { get; set; }
}