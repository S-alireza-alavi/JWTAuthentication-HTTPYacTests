using Authentication.Models;

namespace Authentication;

public static class ApplicationContext
{
    public static ApplicationUser? CurrentUser { get; set; }
}