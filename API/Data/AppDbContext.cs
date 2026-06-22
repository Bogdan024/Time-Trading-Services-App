using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<MemberSkill> MemberSkills { get; set; }
    public DbSet<MemberNeed> MemberNeeds { get; set; }
    public DbSet<MemberAvailabilitySlot> MemberAvailabilitySlots { get; set; }
    public DbSet<TimeTask> TimeTasks { get; set; }
    public DbSet<TimeTransaction> TimeTransactions { get; set; }
    public DbSet<MemberReview> MemberReviews { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageDeletion> MessageDeletions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ServiceCategory>()
            .HasIndex(x => x.Key)
            .IsUnique();

        modelBuilder.Entity<MemberSkill>()
            .HasIndex(x => new { x.MemberId, x.ServiceCategoryId })
            .IsUnique();

        modelBuilder.Entity<MemberNeed>()
            .HasIndex(x => new { x.MemberId, x.ServiceCategoryId })
            .IsUnique();

        modelBuilder.Entity<TimeTask>()
            .HasIndex(x => x.Status);

        modelBuilder.Entity<TimeTask>()
            .HasIndex(x => x.PostedByMemberId);

        modelBuilder.Entity<TimeTask>()
            .HasIndex(x => x.ServiceCategoryId);

        modelBuilder.Entity<TimeTask>()
            .HasOne(x => x.PostedByMember)
            .WithMany()
            .HasForeignKey(x => x.PostedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeTask>()
            .HasOne(x => x.AcceptedByMember)
            .WithMany()
            .HasForeignKey(x => x.AcceptedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeTransaction>()
            .HasIndex(x => x.TimeTaskId)
            .IsUnique();

        modelBuilder.Entity<TimeTransaction>()
            .HasIndex(x => x.FromMemberId);

        modelBuilder.Entity<TimeTransaction>()
            .HasIndex(x => x.ToMemberId);

        modelBuilder.Entity<TimeTransaction>()
            .HasOne(x => x.TimeTask)
            .WithOne(x => x.TimeTransaction)
            .HasForeignKey<TimeTransaction>(x => x.TimeTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeTransaction>()
            .HasOne(x => x.FromMember)
            .WithMany()
            .HasForeignKey(x => x.FromMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeTransaction>()
            .HasOne(x => x.ToMember)
            .WithMany()
            .HasForeignKey(x => x.ToMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MemberReview>()
            .HasIndex(x => new { x.ReviewerMemberId, x.TimeTaskId })
            .IsUnique();

        modelBuilder.Entity<MemberReview>()
            .HasIndex(x => x.ReviewedMemberId);

        modelBuilder.Entity<MemberReview>()
            .HasIndex(x => x.TimeTaskId);

        modelBuilder.Entity<MemberReview>()
            .HasOne(x => x.TimeTask)
            .WithMany()
            .HasForeignKey(x => x.TimeTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MemberReview>()
            .HasOne(x => x.ReviewerMember)
            .WithMany()
            .HasForeignKey(x => x.ReviewerMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MemberReview>()
            .HasOne(x => x.ReviewedMember)
            .WithMany()
            .HasForeignKey(x => x.ReviewedMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Conversation>()
            .HasIndex(x => new { x.Type, x.TimeTaskId })
            .IsUnique();

        modelBuilder.Entity<Conversation>()
            .HasOne(x => x.TimeTask)
            .WithMany()
            .HasForeignKey(x => x.TimeTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConversationParticipant>()
            .HasKey(x => new { x.ConversationId, x.MemberId });

        modelBuilder.Entity<ConversationParticipant>()
            .HasOne(x => x.Conversation)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationParticipant>()
            .HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasIndex(x => x.ConversationId);

        modelBuilder.Entity<Message>()
            .HasOne(x => x.Conversation)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(x => x.SenderMember)
            .WithMany()
            .HasForeignKey(x => x.SenderMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MessageDeletion>()
            .HasKey(x => new { x.MessageId, x.MemberId });

        modelBuilder.Entity<MessageDeletion>()
            .HasOne(x => x.Message)
            .WithMany(x => x.DeletedForMembers)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MessageDeletion>()
            .HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
