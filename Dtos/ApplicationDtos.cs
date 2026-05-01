using System;
using System.ComponentModel.DataAnnotations;

namespace Aihrly.Dtos;

public class CreateApplicationDto
{
    [Required]
    public string CandidateName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
}

public class ApplicationDto
{
    public Guid Id { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ApplicationProfileDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public string Stage { get; set; } = string.Empty;

    public int? CultureFitScore { get; set; }
    public string? CultureFitComment { get; set; }
    public int? InterviewScore { get; set; }
    public string? InterviewComment { get; set; }
    public int? AssessmentScore { get; set; }
    public string? AssessmentComment { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<NoteDto> Notes { get; set; } = new();
    public List<StageHistoryDto> StageHistory { get; set; } = new();
}

public class MoveStageDto
{
    public string TargetStage { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class StageHistoryDto
{
    public string FromStage { get; set; } = string.Empty;
    public string ToStage { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}