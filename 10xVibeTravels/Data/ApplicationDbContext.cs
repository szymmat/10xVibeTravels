using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Interest> Interests { get; set; }
    public virtual DbSet<TravelStyle> TravelStyles { get; set; }
    public virtual DbSet<Intensity> Intensities { get; set; }
    public virtual DbSet<UserProfile> UserProfiles { get; set; }
    public virtual DbSet<UserInterest> UserInterests { get; set; }
    public virtual DbSet<Note> Notes { get; set; }
    public virtual DbSet<Plan> Plans { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- Configure Keys ---
        builder.Entity<UserInterest>().HasKey(ui => new { ui.UserId, ui.InterestId });

        // --- Configure Relationships & Delete Behavior ---
        // UserProfile <-> ApplicationUser (One-to-One)
        builder.Entity<UserProfile>()
            .HasOne(up => up.User)
            .WithOne(u => u.UserProfile)
            .HasForeignKey<UserProfile>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserProfile -> TravelStyles (Many-to-One, Optional)
        builder.Entity<UserProfile>()
            .HasOne(up => up.TravelStyle)
            .WithMany(ts => ts.UserProfiles)
            .HasForeignKey(up => up.TravelStyleId)
            .OnDelete(DeleteBehavior.SetNull); // Set FK to null if TravelStyle is deleted

        // UserProfile -> Intensities (Many-to-One, Optional)
        builder.Entity<UserProfile>()
            .HasOne(up => up.Intensity)
            .WithMany(i => i.UserProfiles)
            .HasForeignKey(up => up.IntensityId)
            .OnDelete(DeleteBehavior.SetNull); // Set FK to null if Intensity is deleted

        // UserInterests (Many-to-Many Junction)
        builder.Entity<UserInterest>()
            .HasOne(ui => ui.User)
            .WithMany(u => u.UserInterests)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete junction entry if User is deleted

        builder.Entity<UserInterest>()
            .HasOne(ui => ui.Interest)
            .WithMany(i => i.UserInterests)
            .HasForeignKey(ui => ui.InterestId)
            .OnDelete(DeleteBehavior.Cascade); // Delete junction entry if Interest is deleted

        // Notes -> User (Many-to-One)
        builder.Entity<Note>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete notes if User is deleted

        // Plans -> User (Many-to-One)
        builder.Entity<Plan>()
            .HasOne(p => p.User)
            .WithMany(u => u.Plans)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete plans if User is deleted

        // --- Configure Defaults & Constraints ---
        builder.Entity<Interest>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
        builder.Entity<TravelStyle>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
        builder.Entity<Intensity>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
        builder.Entity<UserProfile>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
        builder.Entity<Note>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
        builder.Entity<Plan>().Property(e => e.Id).HasDefaultValueSql("NEWID()");

        builder.Entity<UserProfile>().Property(up => up.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Entity<UserProfile>().Property(up => up.ModifiedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Entity<Note>().Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Entity<Note>().Property(n => n.ModifiedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Entity<Plan>().Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Entity<Plan>().Property(p => p.ModifiedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<Plan>()
           .ToTable(tb => tb.HasCheckConstraint("CK_Plans_Status", "[Status] IN ('Generated', 'Accepted', 'Rejected')"));

        // --- Configure Indexes (based on db-plan.md) ---
        builder.Entity<Interest>().HasIndex(i => i.Name).IsUnique();
        builder.Entity<TravelStyle>().HasIndex(ts => ts.Name).IsUnique();
        builder.Entity<Intensity>().HasIndex(i => i.Name).IsUnique();
        // UserProfile(UserId) index created automatically by [Index] attribute / unique FK constraint

        builder.Entity<UserInterest>().HasIndex(ui => ui.InterestId); // Index for lookups by interest

        builder.Entity<Note>().HasIndex(n => n.UserId);
        builder.Entity<Note>().HasIndex(n => n.ModifiedAt); // Descending handled at query time or DB level
        builder.Entity<Note>().HasIndex(n => n.CreatedAt);  // Descending handled at query time or DB level

        builder.Entity<Plan>().HasIndex(p => p.UserId);
        builder.Entity<Plan>().HasIndex(p => p.Status);
        builder.Entity<Plan>().HasIndex(p => p.ModifiedAt); // Descending handled at query time or DB level
        builder.Entity<Plan>().HasIndex(p => p.CreatedAt);  // Descending handled at query time or DB level

        // --- Seed Dictionary Data ---
        SeedDictionaryData(builder);

    }

    private void SeedDictionaryData(ModelBuilder builder)
    {
        // Interests
        builder.Entity<Interest>().HasData(
            new Interest { Id = Guid.Parse("f5b8f6a0-1b1a-4b9a-8f0a-0e1f1b1a0e1a"), Name = "Historia" },
            new Interest { Id = Guid.Parse("f5b8f6a0-1b1a-4b9a-8f0a-0e1f1b1a0e1b"), Name = "Sztuka" },
            new Interest { Id = Guid.Parse("f5b8f6a0-1b1a-4b9a-8f0a-0e1f1b1a0e1c"), Name = "Przyroda" },
            new Interest { Id = Guid.Parse("f5b8f6a0-1b1a-4b9a-8f0a-0e1f1b1a0e1d"), Name = "Życie nocne" },
            new Interest { Id = Guid.Parse("f5b8f6a0-1b1a-4b9a-8f0a-0e1f1b1a0e1e"), Name = "Jedzenie" },
            new Interest { Id = Guid.Parse("f5b8f6a0-1b1a-4b9a-8f0a-0e1f1b1a0e1f"), Name = "Plaże" }
        );

        // Travel Styles
        builder.Entity<TravelStyle>().HasData(
            new TravelStyle { Id = Guid.Parse("e4a7f8b1-2c2b-4c8b-9f1b-1f2a2b2a1f2a"), Name = "Luksusowy" },
            new TravelStyle { Id = Guid.Parse("e4a7f8b1-2c2b-4c8b-9f1b-1f2a2b2a1f2b"), Name = "Budżetowy" },
            new TravelStyle { Id = Guid.Parse("e4a7f8b1-2c2b-4c8b-9f1b-1f2a2b2a1f2c"), Name = "Przygodowy" }
        );

        // Intensities
        builder.Entity<Intensity>().HasData(
            new Intensity { Id = Guid.Parse("d3b6e7c0-3d3c-4d7c-af2c-2e3b3c3b2e3a"), Name = "Relaksacyjny" },
            new Intensity { Id = Guid.Parse("d3b6e7c0-3d3c-4d7c-af2c-2e3b3c3b2e3b"), Name = "Umiarkowany" },
            new Intensity { Id = Guid.Parse("d3b6e7c0-3d3c-4d7c-af2c-2e3b3c3b2e3c"), Name = "Intensywny" }
        );
    }
} 