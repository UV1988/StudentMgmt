using Microsoft.EntityFrameworkCore;
using StudentManager.Api.Domain;

namespace StudentManager.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(100);
            e.Property(s => s.Phone).IsRequired().HasMaxLength(20);
            e.HasIndex(s => s.Name);
        });
    }
}
