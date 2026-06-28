using Cars.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cars.DAL.Entities.Identity
{
    // Розширюю стандартний IdentityUser — не пишу свою систему юзерів,
    // а просто докидаю поля поверх готової Identity-інфраструктури
    public class AppUserEntity : IdentityUser
    {
        // Ці три поля — мої додатки; решта (Email, PasswordHash тощо) йде від IdentityUser
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Image { get; set; }

        // Cascade delete в AppDbContext — при видаленні юзера його токени підуть автоматично
        public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
    }
}