using Microsoft.EntityFrameworkCore;
using TomocaCampaignAPI.Models;

namespace TomocaCampaignAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } // DbSet for User model
        public DbSet<Employee> Employees { get; set; } // DbSet for Employee model
        public DbSet<Transactions> Transactions { get; set; } // DbSet for Transactions model

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Employee -> User relationship with cascade delete
            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Transactions -> Employee relationship with cascade delete
            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.EmployeeDbId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Transactions -> User relationship with cascade delete
            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserDbId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
