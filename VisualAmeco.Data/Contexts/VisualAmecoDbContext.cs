using Microsoft.EntityFrameworkCore;
using VisualAmeco.Core.Entities;
using VisualAmeco.Core.Enums;

namespace VisualAmeco.Data.Contexts;

public class VisualAmecoDbContext : DbContext
{
    public VisualAmecoDbContext(DbContextOptions<VisualAmecoDbContext> options) : base(options)
    {
    }

    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Subchapter> Subchapters => Set<Subchapter>();
    public DbSet<Variable> Variables => Set<Variable>(); // Changed 'Indicators' to 'Variables' for clarity
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

        modelBuilder.Entity<Subchapter>()
            .HasIndex(s => new { s.ChapterId, s.Name })
            .IsUnique();

        modelBuilder.Entity<Value>()
            .HasIndex(v => new { v.VariableId, v.CountryId, v.Year, v.Month })
            .IsUnique();

        modelBuilder.Entity<Subchapter>()
            .HasOne(s => s.Chapter)
            .WithMany(c => c.Subchapters)
            .HasForeignKey(s => s.ChapterId);

        modelBuilder.Entity<Variable>()
            .HasOne(v => v.SubChapter)
            .WithMany(s => s.Variables)
            .HasForeignKey(v => v.SubChapterId);

        modelBuilder.Entity<Value>()
            .HasOne(v => v.Variable)
            .WithMany(v => v.Values)
            .HasForeignKey(v => v.VariableId);

        modelBuilder.Entity<Value>()
            .HasOne(v => v.Country)
            .WithMany(c => c.Values)
            .HasForeignKey(v => v.CountryId);
    }
}