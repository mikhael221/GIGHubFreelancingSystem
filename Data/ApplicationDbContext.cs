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
        public DbSet<ProjectSkill> ProjectSkills { get; set; }
        public DbSet<MentorshipMatch> MentorshipMatches { get; set; }
        public DbSet<MentorshipSession> MentorshipSessions { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<MentorshipGoalCompletion> MentorshipGoalCompletions { get; set; }
        public DbSet<MentorReview> MentorReviews { get; set; }

        // New chat-related entities
        public DbSet<MentorshipChatMessage> MentorshipChatMessages { get; set; }
        public DbSet<MentorshipChatFile> MentorshipChatFiles { get; set; }

        // Notification entity
        public DbSet<Notification> Notifications { get; set; }
        
        // Contract entities
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractAuditLog> ContractAuditLogs { get; set; }
        public DbSet<ContractRevision> ContractRevisions { get; set; }
        public DbSet<ContractTemplate> ContractTemplates { get; set; }
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
                .HasOne(p => p.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.AcceptedBid)
                .WithMany()
                .HasForeignKey(p => p.AcceptedBidId)
                .OnDelete(DeleteBehavior.NoAction);

            // ProjectSkill relationships
            modelBuilder.Entity<ProjectSkill>()
                .HasOne(ps => ps.Project)
                .WithMany(p => p.ProjectSkills)
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectSkill>()
                .HasOne(ps => ps.UserSkill)
                .WithMany()
                .HasForeignKey(ps => ps.UserSkillId)
                .OnDelete(DeleteBehavior.NoAction);

            // Ensure unique project-skill combinations
            modelBuilder.Entity<ProjectSkill>()
                .HasIndex(ps => new { ps.ProjectId, ps.UserSkillId })
                .IsUnique();

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

            // MentorshipSession relationships and indexes
            modelBuilder.Entity<MentorshipSession>()
                .HasOne(ms => ms.MentorshipMatch)
                .WithMany()
                .HasForeignKey(ms => ms.MentorshipMatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MentorshipSession>()
                .HasIndex(ms => ms.MentorshipMatchId);

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

            // MentorshipGoalCompletion relationships and configurations
            modelBuilder.Entity<MentorshipGoalCompletion>()
                .HasOne(mgc => mgc.MentorshipMatch)
                .WithMany()
                .HasForeignKey(mgc => mgc.MentorshipMatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification relationships and configurations
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.UserId);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.CreatedAt);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.IsRead);

            modelBuilder.Entity<Notification>()
                .Property(n => n.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Notification>()
                .Property(n => n.IsRead)
                .HasDefaultValue(false);

            modelBuilder.Entity<MentorshipGoalCompletion>()
                .HasOne(mgc => mgc.Goal)
                .WithMany()
                .HasForeignKey(mgc => mgc.GoalId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MentorshipGoalCompletion>()
                .HasOne(mgc => mgc.CompletedByUser)
                .WithMany()
                .HasForeignKey(mgc => mgc.CompletedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add indexes for better performance
            modelBuilder.Entity<MentorshipGoalCompletion>()
                .HasIndex(mgc => new { mgc.MentorshipMatchId, mgc.GoalId });

            modelBuilder.Entity<MentorshipGoalCompletion>()
                .HasIndex(mgc => mgc.CompletedByUserId);

            modelBuilder.Entity<MentorshipGoalCompletion>()
                .HasIndex(mgc => mgc.CompletedAt);

            // Configure default values
            modelBuilder.Entity<MentorshipGoalCompletion>()
                .Property(mgc => mgc.CompletedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // MentorReview relationships and configurations
            modelBuilder.Entity<MentorReview>()
                .HasOne(mr => mr.MentorshipMatch)
                .WithMany()
                .HasForeignKey(mr => mr.MentorshipMatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MentorReview>()
                .HasOne(mr => mr.Mentor)
                .WithMany()
                .HasForeignKey(mr => mr.MentorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MentorReview>()
                .HasOne(mr => mr.Mentee)
                .WithMany()
                .HasForeignKey(mr => mr.MenteeId)
                .OnDelete(DeleteBehavior.NoAction);

            // Ensure one review per mentorship match
            modelBuilder.Entity<MentorReview>()
                .HasIndex(mr => mr.MentorshipMatchId)
                .IsUnique();

            // Add indexes for better performance
            modelBuilder.Entity<MentorReview>()
                .HasIndex(mr => mr.MentorId);

            modelBuilder.Entity<MentorReview>()
                .HasIndex(mr => mr.MenteeId);

            modelBuilder.Entity<MentorReview>()
                .HasIndex(mr => mr.CreatedAt);

            // Configure default values
            modelBuilder.Entity<MentorReview>()
                .Property(mr => mr.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Contract relationships and configurations
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Project)
                .WithOne(p => p.Contract)
                .HasForeignKey<Contract>(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Bidding)
                .WithMany()
                .HasForeignKey(c => c.BiddingId)
                .OnDelete(DeleteBehavior.NoAction);

            // Contract indexes
            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.ProjectId)
                .IsUnique();

            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.CreatedAt);

            // ContractAuditLog relationships
            modelBuilder.Entity<ContractAuditLog>()
                .HasOne(cal => cal.Contract)
                .WithMany(c => c.AuditLogs)
                .HasForeignKey(cal => cal.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContractAuditLog>()
                .HasOne(cal => cal.User)
                .WithMany()
                .HasForeignKey(cal => cal.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContractAuditLog>()
                .HasIndex(cal => cal.ContractId);

            modelBuilder.Entity<ContractAuditLog>()
                .HasIndex(cal => cal.Timestamp);

            // ContractRevision relationships
            modelBuilder.Entity<ContractRevision>()
                .HasOne(cr => cr.Contract)
                .WithMany(c => c.Revisions)
                .HasForeignKey(cr => cr.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContractRevision>()
                .HasOne(cr => cr.CreatedByUser)
                .WithMany()
                .HasForeignKey(cr => cr.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContractRevision>()
                .HasIndex(cr => new { cr.ContractId, cr.RevisionNumber })
                .IsUnique();

            // ContractTemplate indexes
            modelBuilder.Entity<ContractTemplate>()
                .HasIndex(ct => ct.Category);

            modelBuilder.Entity<ContractTemplate>()
                .HasIndex(ct => ct.IsActive);

            // Removed IsDefault index as it's no longer used

            // Configure default values for contracts
            modelBuilder.Entity<Contract>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ContractAuditLog>()
                .Property(cal => cal.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ContractRevision>()
                .Property(cr => cr.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ContractTemplate>()
                .Property(ct => ct.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            base.OnModelCreating(modelBuilder);
        }
    }
}
