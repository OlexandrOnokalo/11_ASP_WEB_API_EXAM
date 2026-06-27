using Cars.DAL.Repositories;
using Quartz;

namespace Cars.API.Jobs
{
    // Quartz-задача, яка стріляє по неділях о 00:00 —
    // прибираю БД від протермінованих refresh токенів, що ніхто вже не може використати
    public class RefreshTokensCleanupJob : IJob
    {
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<RefreshTokensCleanupJob> _logger;

        public RefreshTokensCleanupJob(
            RefreshTokenRepository refreshTokenRepository,
            ILogger<RefreshTokensCleanupJob> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // 7 днів — рівно TTL refresh токена в JwtService;
            // видаляю тільки ті, що вже протермінували і прожили цей строк — активні не чіпаю
            int deleted = await _refreshTokenRepository.DeleteExpiredOlderThanDaysAsync(7);

            _logger.LogInformation(
                "RefreshTokensCleanupJob виконано. Видалено refresh token: {Count}",
                deleted);
        }
    }
}