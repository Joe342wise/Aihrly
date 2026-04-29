using System;

namespace Aihrly.Models;

public class StageHistory
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string FromStage { get; set; } = string.Empty;
    public string ToStage { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty; // team member Id
    public string Reason { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
