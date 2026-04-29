using System;

namespace Aihrly.Models;

public class ApplicationNote
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // general, screening, interview, reference_check
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public TeamMember? CreatedByUser { get; set; }
}
