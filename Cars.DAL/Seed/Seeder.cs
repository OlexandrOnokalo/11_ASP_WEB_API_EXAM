using Cars.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cars.DAL.Seed
{
    public static class Seeder
    {
        // Оркестратор запуску — викликається один раз при старті з Program.cs
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // Scope потрібен бо Seeder статичний, а DbContext — Scoped-сервіс
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRoleEntity>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUserEntity>>();

            // Застосовую всі незастосовані міграції перед сидінням — БД має бути актуальна
            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);

            // Дані авто та виробників — в окремому класі, щоб не захламляти цей файл
            await DataSeeder.SeedAsync(context);
        }

        // Ідемпотентно — перевіряю наявність перед створенням, можна запускати повторно
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
            // Шукаю за email — якщо вже є, пропускаю; якщо ні — створюю з роллю
            var admin = await userManager.FindByEmailAsync("admin@mail.com");
            if (admin == null)
            {
                admin = new AppUserEntity
                {
                    UserName = "admin",
                    Email = "admin@mail.com",
                    // EmailConfirmed=true одразу — адмін не проходить email-підтвердження
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