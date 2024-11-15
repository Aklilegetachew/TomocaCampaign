using Microsoft.EntityFrameworkCore;
using TomocaCampaignAPI.Models;

namespace TomocaCampaignAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }       // DbSet for User model
        public DbSet<Employee> Employees { get; set; } // DbSet for Employee model
        public DbSet<Transactions> Transactions { get; set; } // DbSet for Employee model
    }
}
