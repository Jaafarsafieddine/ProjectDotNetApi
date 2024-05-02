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
        public DbSet<Car> Products { get; set; }
        public DbSet<AddToCartDetails> AddToCartDetails { get; set;}
        public DbSet<Purchase> Purchase { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
