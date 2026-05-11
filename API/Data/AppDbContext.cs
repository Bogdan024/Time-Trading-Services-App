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
    }
}
