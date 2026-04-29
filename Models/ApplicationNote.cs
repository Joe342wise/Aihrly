using System;

namespace Aihrly.Models;

public class ApplicationNote
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // general, interview_feedback
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
