using Microsoft.EntityFrameworkCore;
using Restward.Api.Models.Entities;
using Environment = Restward.Api.Models.Entities.Environment;

namespace Restward.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<RequestItem> RequestItems => Set<RequestItem>();
    public DbSet<RequestHeader> RequestHeaders => Set<RequestHeader>();
    public DbSet<RequestParameter> RequestParameters => Set<RequestParameter>();
    public DbSet<Environment> Environments => Set<Environment>();
    public DbSet<EnvironmentVariable> EnvironmentVariables => Set<EnvironmentVariable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.ApiKey).IsUnique();
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.Collections)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasOne(e => e.Collection)
                .WithMany(c => c.Folders)
                .HasForeignKey(e => e.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentFolder)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(e => e.ParentFolderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequestItem>(entity =>
        {
            entity.HasOne(e => e.Collection)
                .WithMany(c => c.Requests)
                .HasForeignKey(e => e.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Folder)
                .WithMany(f => f.Requests)
                .HasForeignKey(e => e.FolderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RequestHeader>(entity =>
        {
            entity.HasOne(e => e.RequestItem)
                .WithMany(r => r.Headers)
                .HasForeignKey(e => e.RequestItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequestParameter>(entity =>
        {
            entity.HasOne(e => e.RequestItem)
                .WithMany(r => r.Parameters)
                .HasForeignKey(e => e.RequestItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Environment>(entity =>
        {
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.Environments)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EnvironmentVariable>(entity =>
        {
            entity.HasOne(e => e.Environment)
                .WithMany(env => env.Variables)
                .HasForeignKey(e => e.EnvironmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
