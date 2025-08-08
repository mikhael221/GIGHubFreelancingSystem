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
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<UserAccountSkill> UserAccountSkills { get; set; }
        public DbSet<MentorshipMatch> MentorshipMatches { get; set; }


        // New chat-related entities
        public DbSet<MentorshipChatMessage> MentorshipChatMessages { get; set; }
        public DbSet<MentorshipChatFile> MentorshipChatFiles { get; set; }
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

            modelBuilder.Entity<PeerMentorship>()
                .HasOne(u => u.User)
                .WithOne(p => p.Mentorship)
                .HasForeignKey<PeerMentorship>(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAccountSkill>()
                .HasKey(uas => new { uas.UserAccountId, uas.UserSkillId });

            modelBuilder.Entity<UserAccountSkill>()
                .HasOne(uas => uas.UserAccount)
                .WithMany(ua => ua.UserAccountSkills)
                .HasForeignKey(uas => uas.UserAccountId);

            modelBuilder.Entity<UserAccountSkill>()
                .HasOne(uas => uas.UserSkill)
                .WithMany()
                .HasForeignKey(uas => uas.UserSkillId);

            // New MentorshipMatch relationships
            modelBuilder.Entity<MentorshipMatch>()
                .HasOne(mm => mm.Mentor)
                .WithMany()
                .HasForeignKey(mm => mm.MentorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MentorshipMatch>()
                .HasOne(mm => mm.Mentee)
                .WithMany()
                .HasForeignKey(mm => mm.MenteeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MentorshipMatch>()
                .HasOne(mm => mm.MentorMentorship)
                .WithMany()
                .HasForeignKey(mm => mm.MentorMentorshipId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MentorshipMatch>()
                .HasOne(mm => mm.MenteeMentorship)
                .WithMany()
                .HasForeignKey(mm => mm.MenteeMentorshipId)
                .OnDelete(DeleteBehavior.NoAction);

            // Ensure unique mentor-mentee pairs
            modelBuilder.Entity<MentorshipMatch>()
                .HasIndex(mm => new { mm.MentorId, mm.MenteeId })
                .IsUnique();

            // Add indexes for better performance
            modelBuilder.Entity<MentorshipMatch>()
                .HasIndex(mm => mm.Status);

            modelBuilder.Entity<MentorshipMatch>()
                .HasIndex(mm => mm.MatchedDate);


            // MentorshipChatMessage relationships and configurations
            modelBuilder.Entity<MentorshipChatMessage>()
                .HasOne(mcm => mcm.MentorshipMatch)
                .WithMany()
                .HasForeignKey(mcm => mcm.MentorshipMatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MentorshipChatMessage>()
                .HasOne(mcm => mcm.Sender)
                .WithMany()
                .HasForeignKey(mcm => mcm.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add indexes for chat messages
            modelBuilder.Entity<MentorshipChatMessage>()
                .HasIndex(mcm => mcm.MentorshipMatchId);

            modelBuilder.Entity<MentorshipChatMessage>()
                .HasIndex(mcm => mcm.SenderId);

            modelBuilder.Entity<MentorshipChatMessage>()
                .HasIndex(mcm => mcm.SentAt);

            modelBuilder.Entity<MentorshipChatMessage>()
                .HasIndex(mcm => new { mcm.MentorshipMatchId, mcm.SentAt });

            // MentorshipChatFile relationships
            modelBuilder.Entity<MentorshipChatFile>()
                .HasOne(mcf => mcf.Message)
                .WithMany()
                .HasForeignKey(mcf => mcf.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure default values
            modelBuilder.Entity<MentorshipChatMessage>()
                .Property(mcm => mcm.SentAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<MentorshipChatMessage>()
                .Property(mcm => mcm.IsRead)
                .HasDefaultValue(false);

            modelBuilder.Entity<MentorshipChatMessage>()
                .Property(mcm => mcm.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<MentorshipChatFile>()
                .Property(mcf => mcf.UploadedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            base.OnModelCreating(modelBuilder);
        }
    }
}
