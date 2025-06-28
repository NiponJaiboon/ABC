using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Define DbSets for your entities here
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectSkill> ProjectSkills { get; set; }
    public DbSet<Skill> Skills { get; set; }

    // Step 11: Session Management
    public DbSet<UserSession> UserSessions { get; set; }

    // Step 12: Authorization & Scopes
    public DbSet<OAuthClient> OAuthClients { get; set; }
    public DbSet<UserConsent> UserConsents { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<ScopeDefinitionEntity> ScopeDefinitions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");

            // Configure string properties with max length
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(e => e.ExternalProvider).HasMaxLength(50);
            entity.Property(e => e.ExternalId).HasMaxLength(100);

            // Set default values
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            // Configure relationships
            entity.HasMany(e => e.Portfolios)
                  .WithOne()
                  .HasForeignKey("UserId")
                  .OnDelete(DeleteBehavior.Cascade);

            // Step 11: Configure Sessions relationship
            entity.HasMany(e => e.Sessions)
                  .WithOne(s => s.User)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Step 11: Configure UserSession entity
        builder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions");
            entity.HasKey(e => e.SessionId);

            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.LastAccessed).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 max length
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeviceName).HasMaxLength(100);
            entity.Property(e => e.RefreshToken).HasMaxLength(1000);

            // Index for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.IsActive);
        });

        // Step 12: Configure OAuth Client entity
        builder.Entity<OAuthClient>(entity =>
        {
            entity.ToTable("OAuthClients");
            entity.HasKey(e => e.ClientId);

            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClientType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RedirectUris).IsRequired();
            entity.Property(e => e.PostLogoutRedirectUris);
            entity.Property(e => e.Scopes).IsRequired();
            entity.Property(e => e.GrantTypes);
            entity.Property(e => e.ClientUri).HasMaxLength(500);
            entity.Property(e => e.LogoUri).HasMaxLength(500);
            entity.Property(e => e.ContactEmail).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).IsRequired(false); // Allow null for system clients

            // Indexes
            entity.HasIndex(e => e.ClientName);
            entity.HasIndex(e => e.ClientType);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedBy);

            // Relationships - Optional for system clients
            entity.HasOne(e => e.Creator)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });

        // Step 12: Configure User Consent entity
        builder.Entity<UserConsent>(entity =>
        {
            entity.ToTable("UserConsents");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ClientId).IsRequired();
            entity.Property(e => e.GrantedScopes).IsRequired();
            entity.Property(e => e.RevokedReason).HasMaxLength(500);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => new { e.UserId, e.ClientId });
            entity.HasIndex(e => e.IsRevoked);

            // Relationships
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Client)
                  .WithMany(c => c.UserConsents)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Step 12: Configure User Permission entity
        builder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("UserPermissions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Permission).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Source).HasMaxLength(50);
            entity.Property(e => e.SourceId).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.GrantedBy).IsRequired();

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Permission);
            entity.HasIndex(e => new { e.UserId, e.Permission }).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.ExpiresAt);

            // Relationships
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.GrantedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.GrantedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Step 12: Configure Scope Definition entity
        builder.Entity<ScopeDefinitionEntity>(entity =>
        {
            entity.ToTable("ScopeDefinitions");
            entity.HasKey(e => e.Name);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Permissions).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);

            // Indexes
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDefault);
        });
    }

    private static void ConfigurePortfolioRelationships(ModelBuilder builder)
    {
        builder.Entity<Portfolio>(entity =>
        {
            entity.HasMany(p => p.Projects)
                  .WithOne(p => p.Portfolio)
                  .HasForeignKey(p => p.PortfolioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProjectRelationships(ModelBuilder builder)
    {
        builder.Entity<Project>(entity =>
        {
            entity.HasMany(p => p.ProjectSkills)
                  .WithOne(ps => ps.Project)
                  .HasForeignKey(ps => ps.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSkillRelationships(ModelBuilder builder)
    {
        builder.Entity<Skill>(entity =>
        {
            entity.HasMany(s => s.ProjectSkills)
                  .WithOne(ps => ps.Skill)
                  .HasForeignKey(ps => ps.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
