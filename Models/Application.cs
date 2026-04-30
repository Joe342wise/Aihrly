using System;

namespace Aihrly.Models;

public class Application
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public string Stage { get; set; } = "applied"; // current stage

    public int? CultureFitScore { get; set; }
    public string? CultureFitComment { get; set; }
    public Guid? CultureFitUpdatedBy { get; set; }
    public DateTime? CultureFitUpdatedAt { get; set; }

    public int? InterviewScore { get; set; }
    public string? InterviewComment { get; set; }
    public Guid? InterviewUpdatedBy { get; set; }
    public DateTime? InterviewUpdatedAt { get; set; }

    public int? AssessmentScore { get; set; }
    public string? AssessmentComment { get; set; }
    public Guid? AssessmentUpdatedBy { get; set; }
    public DateTime? AssessmentUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
