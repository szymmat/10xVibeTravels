using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xVibeTravels.Data;

// Junction table for the many-to-many relationship between ApplicationUser and Interest
// Composite key (UserId, InterestId) is configured in DbContext
public class UserInterest
{
    [Required]
    // Foreign Key to AspNetUsers(Id)
    // OnDelete Cascade configured in DbContext
    public required string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    [Required]
    // Foreign Key to Interests(Id)
    // OnDelete Cascade configured in DbContext
    public Guid InterestId { get; set; }

    [ForeignKey(nameof(InterestId))]
    public virtual Interest? Interest { get; set; }
} 