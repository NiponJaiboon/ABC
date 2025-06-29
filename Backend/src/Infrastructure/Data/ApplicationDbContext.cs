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

    // Step 14: Audit & Logging
    public DbSet<AuthenticationAuditLog> AuthenticationAuditLogs { get; set; }
    public DbSet<FailedLoginAttempt> FailedLoginAttempts { get; set; }
    public DbSet<UserActivityAuditLog> UserActivityAuditLogs { get; set; }
    public DbSet<SecurityAuditLog> SecurityAuditLogs { get; set; }

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

        // Step 14: Configure Audit entities
        builder.Entity<AuthenticationAuditLog>(entity =>
        {
            entity.ToTable("AuthenticationAuditLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Result).IsRequired();
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.AuthenticationMethod).HasMaxLength(100);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.AdditionalData).HasMaxLength(1000);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.Result);

            // Foreign key to ApplicationUser
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<FailedLoginAttempt>(entity =>
        {
            entity.ToTable("FailedLoginAttempts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.FailureReason).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AttemptTime).IsRequired();
            entity.Property(e => e.AdditionalData).HasMaxLength(1000);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.AttemptTime);
            entity.HasIndex(e => new { e.IpAddress, e.AttemptTime });

            // Foreign key to ApplicationUser
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<UserActivityAuditLog>(entity =>
        {
            entity.ToTable("UserActivityAuditLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Resource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ResourceId).HasMaxLength(450);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.OldValues).HasMaxLength(1000);
            entity.Property(e => e.NewValues).HasMaxLength(1000);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.Resource);
            entity.HasIndex(e => e.ResourceId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.Resource, e.ResourceId });

            // Foreign key to ApplicationUser
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SecurityAuditLog>(entity =>
        {
            entity.ToTable("SecurityAuditLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Severity).IsRequired();
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RequestPath).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.AdditionalData).HasMaxLength(2000);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Investigated).IsRequired();
            entity.Property(e => e.InvestigationNotes).HasMaxLength(1000);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Investigated);

            // Foreign key to ApplicationUser
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

}
