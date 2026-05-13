using API.Entities;
using Microsoft.EntityFrameworkCore;

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
    }
}
