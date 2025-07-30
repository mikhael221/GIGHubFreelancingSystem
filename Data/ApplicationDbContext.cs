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
        public DbSet<Bidding> Biddings { get; set; }
        public DbSet<PeerMentorship> PeerMentorships { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasOne(p => p.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Bidding>()
                .HasOne(b => b.User)
                .WithMany(u => u.Biddings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Bidding>()
                .HasOne(b => b.Project)
                .WithMany(p => p.Biddings)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bidding>()
                .HasIndex(b => new { b.UserId, b.ProjectId })
                .IsUnique();

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Biddings)
                .WithOne(b => b.Project)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.AcceptedBid)
                .WithMany()
                .HasForeignKey(p => p.AcceptedBidId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.Mentorship)
                .WithOne(p => p.User)
                .HasForeignKey<UserAccount>(u => u.MentorshipId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
