using Microsoft.EntityFrameworkCore;
using ExpenseControl.Api.Entities;

namespace ExpenseControl.Api.Persistence
{
    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Transaction Configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("numeric(18,2)");
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.Type).HasConversion<string>();

                // TPH (Table Per Hierarchy) inheritance for PostgreSQL
                entity.HasDiscriminator<string>("Type");
            });

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconName).HasMaxLength(50);
                entity.Property(e => e.Type).HasConversion<string>();

                // Index on Name and Type for faster lookups
                entity.HasIndex(e => new { e.Name, e.Type }).IsUnique();
            });

            // Budget Configuration
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("numeric(18,2)");
                entity.Property(e => e.Month).IsRequired();
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.CategoryId, e.Month, e.Year }).IsUnique();
            });

            // Relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany() // No navigation property on Category
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.ConfigureSeedData();
        }
    }
}