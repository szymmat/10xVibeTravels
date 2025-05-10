using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xVibeTravels.Data;

public class Note
{
    [Key]
    // Default value configured in DbContext
    public Guid Id { get; set; }

    [Required]
    // Foreign Key to AspNetUsers(Id)
    // MaxLength(450) is implied by IdentityUser's Id type
    // OnDelete Cascade configured in DbContext
    // Index configured in DbContext
    public required string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Content { get; set; }

    [Required]
    // Default value configured in DbContext
    // Index configured in DbContext
    public DateTime CreatedAt { get; set; }

    [Required]
    // Default value configured in DbContext
    // Index configured in DbContext
    public DateTime ModifiedAt { get; set; }
} 