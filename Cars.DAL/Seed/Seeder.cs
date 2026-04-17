using Cars.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cars.DAL.Seed
{
    public static class Seeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRoleEntity>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUserEntity>>();

            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);

            await DataSeeder.SeedAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<AppRoleEntity> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                await roleManager.CreateAsync(new AppRoleEntity { Name = "admin" });
            }

            if (!await roleManager.RoleExistsAsync("user"))
            {
                await roleManager.CreateAsync(new AppRoleEntity { Name = "user" });
            }
        }

        private static async Task SeedUsersAsync(UserManager<AppUserEntity> userManager)
        {
            var admin = await userManager.FindByEmailAsync("admin@mail.com");
            if (admin == null)
            {
                admin = new AppUserEntity
                {
                    UserName = "admin",
                    Email = "admin@mail.com",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Cars"
                };

                await userManager.CreateAsync(admin, "qwerty");
                await userManager.AddToRoleAsync(admin, "admin");
            }

            var user = await userManager.FindByEmailAsync("user@mail.com");
            if (user == null)
            {
                user = new AppUserEntity
                {
                    UserName = "user",
                    Email = "user@mail.com",
                    EmailConfirmed = true,
                    FirstName = "User",
                    LastName = "Cars"
                };

                await userManager.CreateAsync(user, "qwerty");
                await userManager.AddToRoleAsync(user, "user");
            }
        }
    }
}