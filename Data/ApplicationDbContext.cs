using Fynd.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LostItem> LostItems { get; set; }

        public DbSet<FoundItem> FoundItems { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics"
                },
                new Category
                {
                    Id = 2,
                    Name = "Documents"
                },
                new Category
                {
                    Id = 3,
                    Name = "Keys"
                },
                new Category
                {
                    Id = 4,
                    Name = "Wallets"
                },
                new Category
                {
                    Id = 5,
                    Name = "Bags"
                },
                new Category
                {
                    Id = 6,
                    Name = "Clothing"
                },
                new Category
                {
                    Id = 7,
                    Name = "Jewelry"
                },
                new Category
                {
                    Id = 8,
                    Name = "Pets"
                },
                new Category
                {
                    Id = 9,
                    Name = "Other"
                }
            );

            modelBuilder.Entity<LostItem>()
                .HasOne(x => x.User)
                .WithMany(x => x.LostItems)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FoundItem>()
                .HasOne(x => x.User)
                .WithMany(x => x.FoundItems)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
               .HasOne(x => x.User)
               .WithMany(x => x.RefreshTokens)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
               .HasIndex(x => x.TokenHash)
               .IsUnique();
        }


    }
}

