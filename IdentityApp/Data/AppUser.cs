using Microsoft.AspNetCore.Identity;

namespace IdentityApp.Data;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}