using Cars.BLL.Dtos.Auth;
using Cars.BLL.Settings;
using Cars.DAL.Entities;
using Cars.DAL.Entities.Identity;
using Cars.DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Cars.BLL.Services
{
    // Stateless-аутентифікація: видаю пару access+refresh і вмію їх оновити.
    // Refresh зберігаю в БД, бо інакше не можу його інвалідувати до закінчення TTL.
    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUserEntity> _userManager;
        private readonly RefreshTokenRepository _refreshTokenRepository;

        public JwtService(
            IOptions<JwtSettings> jwtOptions,
            UserManager<AppUserEntity> userManager,
            RefreshTokenRepository refreshTokenRepository)
        {
            _jwtSettings = jwtOptions.Value;
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
        }

        // Вхідна точка після успішного логіну/рефрешу — видаю обидва токени разом.
        // ExpiresAtUtc дублюю в DTO, щоб фронт знав коли прийти за рефрешем без декодингу JWT.
        public async Task<JwtDto> GenerateTokensAsync(AppUserEntity user)
        {
            string accessToken = await GenerateAccessTokenAsync(user);
            RefreshTokenEntity refreshToken = GenerateRefreshToken();
            refreshToken.UserId = user.Id;

            // Зберігаю refresh у БД — тільки так потім можу перевірити чи він вже використаний
            await _refreshTokenRepository.CreateAsync(refreshToken);

            return new JwtDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(_jwtSettings.ExpHours)
            };
        }

        // Оновлення пари токенів — тут три причини відмови за однією помилкою навмисно:
        // не хочу підказувати атакуючому яка саме перевірка провалилась.
        public async Task<JwtDto> RefreshAsync(string refreshToken)
        {
            var oldToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            // Перевіряю: існує, не прострочений, не використаний — одноразовий токен
            if (oldToken == null || oldToken.IsExpired || oldToken.IsUsed)
            {
                throw new InvalidOperationException("Refresh token недійсний.");
            }

            // Юзер теоретично міг бути видалений після видачі токена
            var user = await _userManager.FindByIdAsync(oldToken.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("Refresh token недійсний.");
            }

            // Позначаю як використаний до генерації нового — захист від race condition
            oldToken.IsUsed = true;
            await _refreshTokenRepository.UpdateAsync(oldToken);

            return await GenerateTokensAsync(user);
        }

        // Будую сам JWT — claims, ключ, підпис.
        // Кидаю ArgumentNullException тут, бо без ключа токен буде невалідний і краще впасти явно.
        private async Task<string> GenerateAccessTokenAsync(AppUserEntity user)
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                throw new ArgumentNullException(nameof(_jwtSettings.SecretKey), "Jwt secret key is null");
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Стандартні claims + власні (firstName, lastName, image) — фронт читає їх без зайвих запитів
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new("firstName", user.FirstName ?? string.Empty),
                new("lastName", user.LastName ?? string.Empty),
                new("image", user.Image ?? string.Empty)
            };

            // Ролі додаю окремо, бо їх може бути кілька
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // HMAC-SHA256 — симетричне підписання, секрет тільки на сервері
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Генерую непередбачуваний токен через криптографічний RNG, а не System.Random —
        // 64 байти = 512 біт ентропії, brute force нереальний.
        private static RefreshTokenEntity GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            // 7 днів — компроміс між зручністю (не треба часто логінитись) і безпекою.
            // Прострочені токени прибирає RefreshTokensCleanupJob щонеділі.
            return new RefreshTokenEntity
            {
                Token = Convert.ToBase64String(bytes),
                Expires = DateTime.UtcNow.AddDays(7),
                IsUsed = false
            };
        }
    }
}