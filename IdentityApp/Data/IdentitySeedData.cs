using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Data;

public class IdentitySeedData
{
    private const string adminUser = "Admin";
    private const string adminPassword = "Admin_123456";

    public static async void IdentityTestUser(IApplicationBuilder app)
    {
        var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();
        if (context.Database.GetAppliedMigrations().Any())
        {
            context.Database.Migrate();
        }
        var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByNameAsync(adminUser);
        if (user == null)
        {
            user = new AppUser()
            {
                FullName = "Mustafa Comert",
                UserName = adminUser,
                Email = "admin@comert.com",
                PhoneNumber = "44444444"
            };
            await userManager.CreateAsync(user, adminPassword);
        }
    }
}