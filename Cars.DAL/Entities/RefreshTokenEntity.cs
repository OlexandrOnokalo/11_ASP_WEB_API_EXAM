using Cars.DAL.Entities.Identity;

namespace Cars.DAL.Entities
{
    public class RefreshTokenEntity
    {
        public int Id { get; set; }
        // 64 байти через RandomNumberGenerator → Base64 — зберігаю як рядок, в БД є унікальний індекс
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        // Одноразовий токен — після першого обміну позначаю IsUsed=true, повторний запит відхиляю
        public bool IsUsed { get; set; }
        // Computed-властивість, не стовпець в БД — просто зручна перевірка без зайвих запитів
        public bool IsExpired => DateTime.UtcNow > Expires;

        public string UserId { get; set; } = string.Empty;
        public AppUserEntity? User { get; set; }
    }
}