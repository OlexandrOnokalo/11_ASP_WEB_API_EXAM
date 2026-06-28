namespace Cars.BLL.Dtos.Auth
{
    // DTO — пара токенів для клієнта; ExpiresAtUtc — щоб фронт знав коли йти на рефреш без декодування JWT.
    public class JwtDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}