using Microsoft.EntityFrameworkCore;
using ProjectDotNet.Models;

namespace ProjectDotNet.DataServices
{
    public class AppDataContext : DbContext
    {
        public AppDataContext(DbContextOptions<AppDataContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<AddToCart> AddToCarts { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<AddToCartDetails> AddToCartDetails { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AddToCartDetails>(entity =>
            {
                entity.HasOne(d => d.Cart)
                    .WithMany(p => p.AddToCartDetails)
                    .HasForeignKey(d => d.CartId)  // Ensure this uses the new FK name
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Car)
                    .WithMany()
                    .HasForeignKey(d => d.CarId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Cars)
                .WithOne(car => car.Category)
                .HasForeignKey(car => car.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Car)
                .WithMany()
                .HasForeignKey(p => p.CarId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
