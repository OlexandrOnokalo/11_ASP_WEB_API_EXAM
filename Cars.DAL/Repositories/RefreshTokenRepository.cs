using Cars.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cars.DAL.Repositories
{
    public class RefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<RefreshTokenEntity> RefreshTokens => _context.RefreshTokens.AsQueryable();

        public async Task CreateAsync(RefreshTokenEntity entity)
        {
            await _context.RefreshTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RefreshTokenEntity entity)
        {
            _context.RefreshTokens.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshTokenEntity?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<int> DeleteExpiredOlderThanDaysAsync(int days)
        {
            DateTime threshold = DateTime.UtcNow.AddDays(-days);

            var entities = await _context.RefreshTokens
                .Where(x => x.Expires < threshold)
                .ToListAsync();

            if (entities.Count == 0)
            {
                return 0;
            }

            _context.RefreshTokens.RemoveRange(entities);
            await _context.SaveChangesAsync();
            return entities.Count;
        }
    }
}