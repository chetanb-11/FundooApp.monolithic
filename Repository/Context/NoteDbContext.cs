using Microsoft.EntityFrameworkCore;
using NoteService.Domain.Entities;

namespace NoteService.Infrastructure.Context;

public class NoteDbContext(DbContextOptions<NoteDbContext> options) : DbContext(options)
{
    public DbSet<NoteEntity> Notes => Set<NoteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NoteDbContext).Assembly);

        modelBuilder.Entity<NoteEntity>(e =>
        {
            e.Property(n => n.IsArchived).HasDefaultValue(false);
            e.Property(n => n.IsTrashed).HasDefaultValue(false);
            // Previously created implicitly by the FK to Users; kept explicitly for per-user queries.
            e.HasIndex(n => n.UserId);
        });
    }
}
