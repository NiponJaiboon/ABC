using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Profile Information
    public string ProfilePictureUrl { get; set; }
    public string Bio { get; set; }
    public string Website { get; set; }
    public string Location { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // External Provider Information
    public string ExternalProvider { get; set; }
    public string ExternalId { get; set; }

    // Authentication Information
    public DateTime? LastLogin { get; set; }
    public string RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation properties
    public virtual ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
