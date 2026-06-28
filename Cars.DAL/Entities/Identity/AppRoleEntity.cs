using Microsoft.AspNetCore.Identity;

namespace Cars.DAL.Entities.Identity
{
    // Порожній клас навмисно — потрібен, щоб передати свій тип у IdentityDbContext<AppUser, AppRole>.
    // Зараз ролей тільки дві (admin/user), додаткових полів не треба
    public class AppRoleEntity : IdentityRole
    {
    }
}