using System.ComponentModel.DataAnnotations;

namespace _10xVibeTravels.Data;

public class Intensity
{
    [Key]
    // Default value configured in DbContext
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    // Unique constraint configured in DbContext
    public required string Name { get; set; }

    // Navigation Property
    public virtual ICollection<UserProfile> UserProfiles { get; set; } = new List<UserProfile>();
} 