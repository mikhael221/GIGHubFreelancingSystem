using Freelancing.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Freelancing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
    }
}
