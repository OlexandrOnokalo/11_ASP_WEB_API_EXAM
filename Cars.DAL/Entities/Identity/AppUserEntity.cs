using Cars.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cars.DAL.Entities.Identity
{
    public class AppUserEntity : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Image { get; set; }

        public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
    }
}