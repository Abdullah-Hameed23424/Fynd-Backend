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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}