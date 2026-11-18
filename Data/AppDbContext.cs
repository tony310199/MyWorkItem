using Microsoft.EntityFrameworkCore;
using WorkItemApi.Models.Entities;

namespace WorkItemApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<UserWorkItemStatus> UserWorkItemStatuses => Set<UserWorkItemStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.LoginName).IsUnique();
            entity.Property(x => x.LoginName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.UserName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.HasIndex(x => x.IsDeleted);
        });

        modelBuilder.Entity<UserWorkItemStatus>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.WorkItemId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.WorkItemStatuses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.WorkItem)
                .WithMany(x => x.UserStatuses)
                .HasForeignKey(x => x.WorkItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTimeOffset(2026, 11, 18, 9, 0, 0, TimeSpan.FromHours(8));
        var adminId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var aliceId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var bobId = Guid.Parse("10000000-0000-0000-0000-000000000003");

        modelBuilder.Entity<AppUser>().HasData(
            new AppUser { Id = adminId, LoginName = "admin", UserName = "管理者", Role = UserRole.Admin, CreatedAt = now },
            new AppUser { Id = aliceId, LoginName = "alice", UserName = "Alice", Role = UserRole.User, CreatedAt = now },
            new AppUser { Id = bobId, LoginName = "bob", UserName = "Bob", Role = UserRole.User, CreatedAt = now });

        modelBuilder.Entity<WorkItem>().HasData(
            new WorkItem { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Title = "完成面試報告", Description = "整理 .NET、EF Core 與前端架構", Category = "面試準備", Priority = WorkItemPriority.High, DueDate = now.AddDays(7), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new WorkItem { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), Title = "準備系統設計題", Description = "練習分層架構與 RESTful API", Category = "面試準備", Priority = WorkItemPriority.Medium, DueDate = now.AddDays(5), IsDeleted = false, CreatedAt = now, UpdatedAt = now },
            new WorkItem { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), Title = "複習 SQL 與 EF Core", Description = "關聯、交易與效能概念", Category = "技術複習", Priority = WorkItemPriority.Low, DueDate = null, IsDeleted = false, CreatedAt = now, UpdatedAt = now });

        modelBuilder.Entity<UserWorkItemStatus>().HasData(
            new UserWorkItemStatus { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), UserId = aliceId, WorkItemId = Guid.Parse("20000000-0000-0000-0000-000000000001"), IsChecked = true, IsConfirmed = true, LastChangedAt = now, ConfirmedAt = now });
    }
}
