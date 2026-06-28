using Cars.API.Models;

namespace Cars.API.Middlewares
{
    // Централізована обробка виключень — без неї кожен контролер мав би свій try/catch; сервіси просто кидають виключення, а тут воно перетворюється на HTTP-відповідь
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // передаю управління решті пайплайна: auth middleware, контролер, сервіс
            }
            // Невалідні вхідні дані — напр.: manufactureId не існує; LogWarning бо це фаул клієнта, не сервера
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto { Message = ex.Message });
            }
            // Порушення бізнес-правил — неправильний пароль, токен вже використаний, юзер не знайдений тощо
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto { Message = ex.Message });
            }
            // Несподівана помилка — LogError (не Warning), треба розслідувати
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                // Навмисно без деталей: стек не повинен іти клієнту — повна інформація є в логах
                await context.Response.WriteAsJsonAsync(new ErrorResponseDto { Message = "Internal server error." });
            }
        }
    }
}