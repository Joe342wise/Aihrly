using System;

namespace Aihrly.Models;

public class StageHistory
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string FromStage { get; set; } = string.Empty;
    public string ToStage { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }

    public TeamMember? ChangedByUser { get; set; }
}
