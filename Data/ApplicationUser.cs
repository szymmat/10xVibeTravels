using Microsoft.AspNetCore.Identity;

namespace _10xVibeTravels.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    // Navigation Properties
    public virtual UserProfile? UserProfile { get; set; }
    public virtual ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    public virtual ICollection<Plan> Plans { get; set; } = new List<Plan>();
} 