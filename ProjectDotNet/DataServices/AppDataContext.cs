using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectDotNet.Models;

namespace ProjectDotNet.DataServices
{
    public class AppDataContext : IdentityDbContext<IdentityUser>
    {
        public AppDataContext(DbContextOptions<AppDataContext> options) : base(options)
        {
        }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AddToCart> AddToCarts { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<AddToCartDetails> AddToCartDetails { get; set;}
        public DbSet<Purchase> Purchase { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            
            modelBuilder.Entity<AddToCartDetails>(entity =>
            {
                entity.HasOne(d => d.cart)
                    .WithMany()
                    .HasForeignKey(d => d.cartId)
                    .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(d => d.car)
                    .WithMany()
                    .HasForeignKey(d => d.carId)
                    .OnDelete(DeleteBehavior.Cascade); 
            });
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Cars)  
                .WithOne(car => car.category)  
                .HasForeignKey(car => car.categoryId)  
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.user)
                .WithMany()  
                .HasForeignKey(p => p.userId)
                .OnDelete(DeleteBehavior.Restrict);  

            
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.car)
                .WithMany()  
                .HasForeignKey(p => p.carId)
                .OnDelete(DeleteBehavior.Restrict);
        }   
    }
}
