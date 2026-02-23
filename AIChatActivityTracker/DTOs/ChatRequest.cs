using System.ComponentModel.DataAnnotations;

namespace AIChatActivityTracker.DTOs;

public class ChatRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}
