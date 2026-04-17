using Cars.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cars.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ManufactureEntity> Manufactures { get; set; }
        public DbSet<CarEntity> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

                e.HasOne(x => x.Manufacture)
                    .WithMany(x => x.Cars)
                    .HasForeignKey(x => x.ManufactureId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}