using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Data;

[Index(nameof(UserId), IsUnique = true)] // Ensures one-to-one with ApplicationUser
public class UserProfile
{
    [Key]
    // Default value configured in DbContext
    public Guid Id { get; set; }

    [Required]
    // Foreign Key to AspNetUsers(Id)
    // MaxLength(450) is implied by IdentityUser's Id type
    // Unique constraint configured via [Index]
    // OnDelete Cascade configured in DbContext
    public required string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? Budget { get; set; }

    // Foreign Key to TravelStyles(Id)
    // OnDelete Set Null configured in DbContext
    public Guid? TravelStyleId { get; set; }

    [ForeignKey(nameof(TravelStyleId))]
    public virtual TravelStyle? TravelStyle { get; set; }

    // Foreign Key to Intensities(Id)
    // OnDelete Set Null configured in DbContext
    public Guid? IntensityId { get; set; }

    [ForeignKey(nameof(IntensityId))]
    public virtual Intensity? Intensity { get; set; }

    [Required]
    // Default value configured in DbContext
    public DateTime CreatedAt { get; set; }

    [Required]
    // Default value configured in DbContext
    public DateTime ModifiedAt { get; set; }
} 