using Cars.DAL.Entities.Identity;

namespace Cars.DAL.Entities
{
    public class RefreshTokenEntity
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsUsed { get; set; }
        public bool IsExpired => DateTime.UtcNow > Expires;

        public string UserId { get; set; } = string.Empty;
        public AppUserEntity? User { get; set; }
    }
}