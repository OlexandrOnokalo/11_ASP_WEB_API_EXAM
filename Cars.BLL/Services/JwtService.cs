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

        // вхідна точка генерації пари токенів — викликається і при логіні, і при рефреші
        public async Task<JwtDto> GenerateTokensAsync(AppUserEntity user)
        {
            string accessToken = await GenerateAccessTokenAsync(user);
            RefreshTokenEntity refreshToken = GenerateRefreshToken();
            // UserId задаю після генерації, бо GenerateRefreshToken() статичний і не знає про юзера
            refreshToken.UserId = user.Id;

            await _refreshTokenRepository.CreateAsync(refreshToken);

            return new JwtDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                // ExpiresAtUtc — зручність для клієнта, щоб він знав коли рефрешити без парсингу JWT
                ExpiresAtUtc = DateTime.UtcNow.AddHours(_jwtSettings.ExpHours)
            };
        }

        // ротація refresh токена: старий помічаю як використаний, видаю нову пару
        public async Task<JwtDto> RefreshAsync(string refreshToken)
        {
            var oldToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            // три причини відмови в одному if — навмисно одне й те саме повідомлення,
            // щоб зловмисник не міг визначити чому саме токен не пройшов
            if (oldToken == null || oldToken.IsExpired || oldToken.IsUsed)
            {
                throw new InvalidOperationException("Refresh token недійсний.");
            }

            var user = await _userManager.FindByIdAsync(oldToken.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("Refresh token недійсний.");
            }

            // позначаю як використаний ДО генерації нового — захист від повторного використання
            oldToken.IsUsed = true;
            await _refreshTokenRepository.UpdateAsync(oldToken);

            return await GenerateTokensAsync(user);
        }

        private async Task<string> GenerateAccessTokenAsync(AppUserEntity user)
        {
            // падаю одразу, бо без ключа далі буде незрозуміла крипто-помилка
            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                throw new ArgumentNullException(nameof(_jwtSettings.SecretKey), "Jwt secret key is null");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                // firstName, lastName, image — нестандартні claims, потрібні фронтенду без окремого запиту
                new("firstName", user.FirstName ?? string.Empty),
                new("lastName", user.LastName ?? string.Empty),
                new("image", user.Image ?? string.Empty)
            };

            // ролей може бути кілька — додаю кожну окремим claim, так ASP.NET їх і очікує
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // HMAC-SHA256 — симетричне підписання, той самий ключ і для підпису, і для перевірки
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

        private static RefreshTokenEntity GenerateRefreshToken()
        {
            // 64 байти через криптографічно стійкий RNG — не Random, а справжня ентропія
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return new RefreshTokenEntity
            {
                Token = Convert.ToBase64String(bytes),
                // 7 днів — збігається з інтервалом очищення в Quartz job (DeleteExpiredOlderThanDaysAsync(7))
                Expires = DateTime.UtcNow.AddDays(7),
                IsUsed = false
            };
        }
    }
}