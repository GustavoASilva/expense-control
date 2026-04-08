using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ExpenseControl.Api.Entities;

namespace ExpenseControl.Api.Persistence;

public class ExpenseDbContext(DbContextOptions<ExpenseDbContext> options, IEncryptionService encryptionService) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Saving> Savings { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdMember> HouseholdMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Value converters for encrypted fields
        var encryptedString = new ValueConverter<string, string>(
            v => encryptionService.Encrypt(v),
            v => encryptionService.Decrypt(v));

        var encryptedNullableString = new ValueConverter<string?, string?>(
            v => v == null ? null : encryptionService.Encrypt(v),
            v => v == null ? null : encryptionService.Decrypt(v));

        var encryptedDecimal = new ValueConverter<decimal, string>(
            v => encryptionService.Encrypt(v.ToString(CultureInfo.InvariantCulture)),
            v => decimal.Parse(encryptionService.Decrypt(v), CultureInfo.InvariantCulture));

        var encryptedNullableDecimal = new ValueConverter<decimal?, string?>(
            v => v == null ? null : encryptionService.Encrypt(v.Value.ToString(CultureInfo.InvariantCulture)),
            v => v == null ? null : decimal.Parse(encryptionService.Decrypt(v), CultureInfo.InvariantCulture));

        // Household Configuration
        modelBuilder.Entity<Household>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.InviteId).IsRequired();
            entity.HasIndex(e => e.InviteId).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // HouseholdMember Configuration
        modelBuilder.Entity<HouseholdMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.JoinedAt).IsRequired();
            entity.HasOne(e => e.Household)
                .WithMany()
                .HasForeignKey(e => e.HouseholdId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.HouseholdId, e.UserId }).IsUnique();
        });

        // Transaction Configuration
        // Amount, Description, and Notes are encrypted at rest; stored as text columns
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasConversion(encryptedDecimal);
            entity.Property(e => e.Description).IsRequired().HasConversion(encryptedNullableString);
            entity.Property(e => e.Notes).HasConversion(encryptedNullableString);
            entity.Property(e => e.Type).HasConversion<string>();

            // TPH (Table Per Hierarchy) inheritance for PostgreSQL
            entity.HasDiscriminator<string>("Type");

            entity.HasOne(e => e.Household)
                .WithMany()
                .HasForeignKey(e => e.HouseholdId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.HouseholdId);
        });

        // Category Configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(50);
            entity.Property(e => e.Type).HasConversion<string>();

            entity.HasOne(e => e.Household)
                .WithMany()
                .HasForeignKey(e => e.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique index on Name, Type, and HouseholdId for per-household uniqueness
            entity.HasIndex(e => new { e.Name, e.Type, e.HouseholdId }).IsUnique();
        });

        // Budget Configuration
        // Amount is encrypted at rest; stored as a text column
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasConversion(encryptedDecimal);
            entity.Property(e => e.Month).IsRequired();
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Household)
                .WithMany()
                .HasForeignKey(e => e.HouseholdId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CategoryId, e.Month, e.Year, e.HouseholdId }).IsUnique();
            entity.HasIndex(e => e.HouseholdId);
        });

        // Saving Configuration
        // Name, Description, CurrentAmount, and TargetAmount are encrypted at rest
        modelBuilder.Entity<Saving>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasConversion(encryptedString);
            entity.Property(e => e.Description).HasConversion(encryptedNullableString);
            entity.Property(e => e.CurrentAmount).HasConversion(encryptedDecimal);
            entity.Property(e => e.TargetAmount).HasConversion(encryptedNullableDecimal);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasOne(e => e.Household)
                .WithMany()
                .HasForeignKey(e => e.HouseholdId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.HouseholdId);
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