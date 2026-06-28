namespace Cars.BLL.Settings
{
    // POCO для IOptions<JwtSettings> — зчитується з appsettings.json секція "JwtSettings".
    // Реєструю в DI через AddJwtAuthentication() у DependencyInjectionExtensions.
    public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        // SecretKey тримаю поза репозиторієм (env/secrets) — якщо порожній, JwtService кине виняток
        public string SecretKey { get; set; } = string.Empty;
        // 1 година за замовчуванням — короткий TTL, далі юзер іде за refresh
        public int ExpHours { get; set; } = 1;
    }
}