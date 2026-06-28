using Cars.DAL.Repositories;
using Quartz;

namespace Cars.API.Jobs
{
    // Quartz викликає Execute за CRON (0 0 0 ? * SUN); розклад зареєстровано в AddJobs в Program.cs
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
            // 7 — той самий TTL що й при видачі в JwtService; видаляю після протерміну — невеличкий grace period на випадок діагностики
            int deleted = await _refreshTokenRepository.DeleteExpiredOlderThanDaysAsync(7);

            // Логую кількість видалених — зручно побачити чи задача взагалі запускалась і що видалила
            _logger.LogInformation(
                "RefreshTokensCleanupJob виконано. Видалено refresh token: {Count}",
                deleted);
        }
    }
}