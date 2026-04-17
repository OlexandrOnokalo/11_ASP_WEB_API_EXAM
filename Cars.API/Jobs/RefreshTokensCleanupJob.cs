using Cars.DAL.Repositories;
using Quartz;

namespace Cars.API.Jobs
{
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
            int deleted = await _refreshTokenRepository.DeleteExpiredOlderThanDaysAsync(7);

            _logger.LogInformation(
                "RefreshTokensCleanupJob виконано. Видалено refresh token: {Count}",
                deleted);
        }
    }
}