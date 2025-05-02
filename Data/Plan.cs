using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _10xVibeTravels.Data;

public class Plan
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
    [MaxLength(20)]
    // Check constraint configured in DbContext
    // Index configured in DbContext
    public required string Status { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime EndDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(9, 2)")]
    public decimal Budget { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Title { get; set; }

    [Required]
    // Type nvarchar(max) configured via convention or DbContext
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