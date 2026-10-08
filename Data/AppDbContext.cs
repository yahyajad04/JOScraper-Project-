using JoSale.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JoSale.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<StoreItem> StoreItem { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = "admin-role-id",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "user-role-id",
                    Name = "User",
                    NormalizedName = "USER"
                },
            };
            builder.Entity<IdentityRole>().HasData(roles);

            builder.Entity<StoreItem>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            builder.Entity<StoreItem>()
                .HasKey(si => new { si.StoreId, si.ItemId });

            builder.Entity<StoreItem>()
                .HasIndex(si => si.StoreId);

            builder.Entity<Item>()
                .HasIndex(i => i.NormalizedName)
                .IsUnique();

            builder.Entity<Item>()
                    .HasIndex(i => i.CategoryId);
        }
    }
}
