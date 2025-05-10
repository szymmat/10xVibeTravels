using System.ComponentModel.DataAnnotations;

namespace _10xVibeTravels.Requests;

public class UpdateNoteRequest
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
} 