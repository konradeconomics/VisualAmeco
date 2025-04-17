using Microsoft.EntityFrameworkCore;
using VisualAmeco.Core.Entities;

namespace VisualAmeco.Data.Contexts;

public class VisualAmecoDbContext : DbContext
{
    public VisualAmecoDbContext(DbContextOptions<VisualAmecoDbContext> options) : base(options)
    {
    }
    
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Subchapter> Subchapters => Set<Subchapter>();
    public DbSet<Variable> Indicators => Set<Variable>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Value> Values => Set<Value>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Variable>()
            .HasIndex(i => i.Code)
            .IsUnique();

        modelBuilder.Entity<Country>()
            .HasIndex(c => c.Code)
            .IsUnique();
    }
}