using Cars.BLL.Dtos.Auth;
using Cars.BLL.Dtos.Common;
using Cars.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cars.API.Controllers
{
    // Всі ендпоінти анонімні — [Authorize] немає, бо авторизованому юзеру нема сенсу логінитись чи реєструватись
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;

        // AuthService — робота з Identity (юзери, ролі, email)
        // JwtService — чисто токени: генерація і оновлення; розділив бо Identity-логіка не залежить від JWT
        public AuthController(AuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        // Повертає confirmation token, а не JWT — юзер повинен спочатку підтвердити email
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(new ApiResponseDto<RegisterResultDto> { Data = result });
        }

        // Повертає AuthResultDto: одразу і токени (access+refresh), і дані юзера — щоб фронт не робив другий запит
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(new ApiResponseDto<AuthResultDto> { Data = result });
        }

        // Тут викликаю _jwtService, а не _authService — refresh це чисто JWT-операція, Identity тут не нужна
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _jwtService.RefreshAsync(dto.RefreshToken);
            return Ok(new ApiResponseDto<JwtDto> { Data = result });
        }

        // GET — бо посилання відкривається прямо з браузера, всі параметри в URL; токен у URL — URL-decode робить AuthService
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string token)
        {
            await _authService.ConfirmEmailAsync(userId, token);
            return Ok(new ApiResponseDto<object> { Data = new { message = "Email успішно підтверджено." } });
        }
    }
}