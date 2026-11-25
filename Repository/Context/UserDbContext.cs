using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Context;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);

        modelBuilder.Entity<UserEntity>(e =>
        {
            e.Property(u => u.Role).HasDefaultValue("User").HasMaxLength(20);
            e.HasIndex(u => u.Email).IsUnique();
        });
    }
}
