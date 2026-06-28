using Cars.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cars.DAL.Repositories
{
    // Репозиторій тільки для RefreshToken — Car і Manufacture доступні напряму через context у сервісах,
    // тут окремий клас бо токени мають специфічну логіку (cleanup, one-time use)
    public class RefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        // Відкриваю IQueryable назовні — JwtService може сам доліпити Include чи фільтр
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

        // Шукаю за точним значенням токена — в БД є унікальний індекс, тому FirstOrDefault достатньо
        public async Task<RefreshTokenEntity?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        // Викликається Quartz-джобою щонеділі — видаляю протерміновані токени,
        // які вже старші за `days` днів; повертаю кількість для логу
        public async Task<int> DeleteExpiredOlderThanDaysAsync(int days)
        {
            DateTime threshold = DateTime.UtcNow.AddDays(-days);

            var entities = await _context.RefreshTokens
                .Where(x => x.Expires < threshold)
                .ToListAsync();

            // Якщо нема що видаляти — не йду в SaveChanges зайвий раз
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