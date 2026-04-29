using System;
using Aihrly.Models;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Data;

public class AihrlyDbContext : DbContext
{
    public AihrlyDbContext(DbContextOptions<AihrlyDbContext> options)
        : base(options) { }
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationNote> ApplicationNotes => Set<ApplicationNote>();
    public DbSet<StageHistory> StageHistories => Set<StageHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraint: same email can't apply to the same job twice
        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.JobId, a.CandidateEmail })
            .IsUnique();

        // Job -> Applications
        modelBuilder.Entity<Application>()
            .HasOne<Job>()
            .WithMany()
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Application -> Notes
        modelBuilder.Entity<ApplicationNote>()
            .HasOne<Application>()
            .WithMany()
            .HasForeignKey(n => n.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Application -> StageHistory
        modelBuilder.Entity<StageHistory>()
            .HasOne<Application>()
            .WithMany()
            .HasForeignKey(s => s.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Note -> TeamMember (author)
        modelBuilder.Entity<ApplicationNote>()
            .HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(n => n.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // StageHistory → TeamMember (changed_by)
        modelBuilder.Entity<StageHistory>()
            .HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(h => h.ChangedBy)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Score attribution FKs
        modelBuilder.Entity<Application>()
            .HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(a => a.CultureFitUpdatedBy)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Application>()
            .HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(a => a.InterviewUpdatedBy)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Application>()
            .HasOne<TeamMember>()
            .WithMany()
            .HasForeignKey(a => a.AssessmentUpdatedBy)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for query performance
        modelBuilder.Entity<ApplicationNote>()
            .HasIndex(n => n.ApplicationId);

        modelBuilder.Entity<StageHistory>()
            .HasIndex(h => h.ApplicationId);

        // Value constraints
        modelBuilder.Entity<Job>()
            .Property(j => j.Status)
            .HasMaxLength(20);

        modelBuilder.Entity<Application>()
            .Property(a => a.Stage)
            .HasMaxLength(30);

        modelBuilder.Entity<ApplicationNote>()
            .Property(n => n.Type)
            .HasMaxLength(30);

        modelBuilder.Entity<TeamMember>()
            .Property(t => t.Role)
            .HasMaxLength(30);

        // Seed team members
        modelBuilder.Entity<TeamMember>().HasData(
            new TeamMember { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Sarah Chen", Email = "sarah@aihrly.com", Role = "recruiter" },
            new TeamMember { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Marcus Johnson", Email = "marcus@aihrly.com", Role = "hiring_manager" },
            new TeamMember { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Priya Patel", Email = "priya@aihrly.com", Role = "recruiter" }
        );
    }
}
