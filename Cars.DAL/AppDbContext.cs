using Cars.DAL.Entities;
using Cars.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cars.DAL
{
    // Єдиний контекст бази — успадковую IdentityDbContext, щоб не дублювати
    // таблиці Identity вручну; string як тип ключа — стандарт для ASP.NET Identity
    public class AppDbContext : IdentityDbContext<AppUserEntity, AppRoleEntity, string>
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        // Identity-таблиці (Users, Roles тощо) успадковую з базового класу
        public DbSet<ManufactureEntity> Manufactures { get; set; }
        public DbSet<CarEntity> Cars { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // КРИТИЧНО: без цього виклику Identity-таблиці не налаштуються
            base.OnModelCreating(builder);

            // Виробника не можна дублювати — унікальний індекс захищає краще за перевірку в сервісі
            builder.Entity<ManufactureEntity>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                e.HasIndex(x => x.Name)
                    .IsUnique();
            });

            builder.Entity<CarEntity>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(120);

                e.Property(x => x.Year)
                    .IsRequired();

                // numeric(p,s) замість double — PostgreSQL зберігає точно,
                // без проблем із плаваючою комою при роботі з грошима
                e.Property(x => x.Volume)
                    .IsRequired()
                    .HasColumnType("numeric(10,2)");

                e.Property(x => x.Price)
                    .IsRequired()
                    .HasColumnType("numeric(12,2)");

                e.Property(x => x.Color)
                    .IsRequired()
                    .HasMaxLength(40);

                e.Property(x => x.Description)
                    .HasColumnType("text");

                e.Property(x => x.Image)
                    .HasMaxLength(500);

                // Restrict, а не Cascade — не даю видалити виробника поки є авто;
                // інакше каскад знесе весь каталог разом із виробником
                e.HasOne(x => x.Manufacture)
                    .WithMany(x => x.Cars)
                    .HasForeignKey(x => x.ManufactureId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<RefreshTokenEntity>(e =>
            {
                e.HasKey(x => x.Id);

                // Унікальний індекс на токен — шукаю по ньому при кожному refresh,
                // тому індекс тут не розкіш, а необхідність
                e.Property(x => x.Token)
                    .IsRequired()
                    .HasMaxLength(512);

                e.HasIndex(x => x.Token)
                    .IsUnique();

                e.Property(x => x.Expires)
                    .IsRequired();

                e.Property(x => x.IsUsed)
                    .IsRequired();

                e.Property(x => x.UserId)
                    .IsRequired();

                // Тут навмисно Cascade — токени безглузді без юзера,
                // тому при видаленні акаунта чистяться автоматично
                e.HasOne(x => x.User)
                    .WithMany(x => x.RefreshTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}