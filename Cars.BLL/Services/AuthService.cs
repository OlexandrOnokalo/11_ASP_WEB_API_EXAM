using Cars.BLL.Dtos.Auth;
using Cars.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Cars.BLL.Services
{
    public class AuthService
    {
        private readonly UserManager<AppUserEntity> _userManager;
        private readonly RoleManager<AppRoleEntity> _roleManager;
        private readonly JwtService _jwtService;

        public AuthService(
            UserManager<AppUserEntity> userManager,
            RoleManager<AppRoleEntity> roleManager,
            JwtService jwtService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        // вхідна точка реєстрації: перевіряю унікальність, створюю юзера, видаю токен для підтвердження email
        public async Task<RegisterResultDto> RegisterAsync(RegisterDto dto)
        {
            // перевіряю email і username окремо, щоб дати точне повідомлення що саме зайняте
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
            {
                throw new InvalidOperationException($"Пошта '{dto.Email}' вже використовується.");
            }

            if (await _userManager.FindByNameAsync(dto.UserName) != null)
            {
                throw new InvalidOperationException($"Ім'я користувача '{dto.UserName}' зайняте.");
            }

            var user = new AppUserEntity
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            // Identity сам хешує пароль — передаю сирий рядок, назовні він не зберігається
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(createResult.Errors.First().Description);
            }

            // резерв на випадок якщо сидер не відпрацював — роль "user" має бути, але підстрахуюсь
            if (!await _roleManager.RoleExistsAsync("user"))
            {
                await _roleManager.CreateAsync(new AppRoleEntity { Name = "user" });
            }

            await _userManager.AddToRoleAsync(user, "user");

            // токен підтвердження може містити +, /, = — кодую, бо він піде в URL-параметр
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string encodedToken = Uri.EscapeDataString(token);

            return new RegisterResultDto
            {
                Message = "Реєстрація успішна. Підтвердіть email через /api/auth/confirm-email",
                UserId = user.Id,
                ConfirmationToken = encodedToken
            };
        }

        // вхідна точка логіну: знаходжу юзера, перевіряю пароль, делегую генерацію токенів у JwtService
        public async Task<AuthResultDto> LoginAsync(LoginDto dto)
        {
            // шукаю по email, бо username не завжди знають — так зручніше
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new InvalidOperationException($"Користувач з поштою '{dto.Email}' не існує.");
            }

            // Identity порівнює хеш сам — не торкаюсь паролю напряму
            bool passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                throw new InvalidOperationException("Пароль вказано невірно.");
            }

            // роли потрібні і для claims у токені, і для відповіді клієнту — тягну окремо
            var tokens = await _jwtService.GenerateTokensAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResultDto
            {
                Tokens = tokens,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToArray()
                }
            };
        }

        // підтвердження email: розкодовую токен з URL і передаю Identity — він сам ставить EmailConfirmed = true
        public async Task ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Користувача не знайдено.");
            }

            // токен прийшов URL-encoded з query string — розкодовую назад перед передачею в Identity
            string decodedToken = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Errors.First().Description);
            }
        }
    }
}