using System;
using Aihrly.Data;
using Aihrly.Dtos;
using Aihrly.Filters;
using Aihrly.Models;
using Aihrly.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly AihrlyDbContext _context;

    public ApplicationsController(AihrlyDbContext context)
    {
        _context = context;
    }

    // POST /api/jobs/{jobId}/applications
    [HttpPost("/api/jobs/{jobId}/applications")]
    public async Task<IActionResult> Apply(Guid jobId, [FromBody] CreateApplicationDto dto)
    {
        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null)
            return Problem($"Job with id '{jobId}' not found.", statusCode: 404);

        if (string.IsNullOrWhiteSpace(dto.CandidateName))
            return Problem("Candidate name is required.", statusCode: 400);

        if (string.IsNullOrWhiteSpace(dto.CandidateEmail))
            return Problem("Candidate email is required.", statusCode: 400);

        var application = new Application
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            CandidateName = dto.CandidateName,
            CandidateEmail = dto.CandidateEmail,
            CoverLetter = dto.CoverLetter,
            Stage = "applied",
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Problem("A candidate with this email has already applied to this job.", statusCode: 409);
        }

        var result = new ApplicationDto
        {
            Id = application.Id,
            CandidateName = application.CandidateName,
            CandidateEmail = application.CandidateEmail,
            Stage = application.Stage,
            CreatedAt = application.CreatedAt,
        };

        return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, result);
    }

    // GET /api/jobs/{jobId}/applications
    [HttpGet("/api/jobs/{jobId}/applications")]
    public async Task<IActionResult> ListApplications(Guid jobId, [FromQuery] string? stage)
    {
        var job = await _context.Jobs.FindAsync(jobId);
        if (job == null)
            return Problem($"Job with id '{jobId}' not found.", statusCode: 404);

        var query = _context.Applications.Where(a => a.JobId == jobId);

        if (!string.IsNullOrWhiteSpace(stage))
            query = query.Where(a => a.Stage == stage);

        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var dtos = applications.Select(a => new ApplicationDto
        {
            Id = a.Id,
            CandidateName = a.CandidateName,
            CandidateEmail = a.CandidateEmail,
            Stage = a.Stage,
            CreatedAt = a.CreatedAt,
        }).ToList();

        return Ok(dtos);
    }

    // GET /api/applications/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound($"Application with id '{id}' not found.");

        var notes = await _context.ApplicationNotes
            .Include(n => n.CreatedByUser)
            .Where(n => n.ApplicationId == id)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var stageHistory = await _context.StageHistories
            .Include(h => h.ChangedByUser)
            .Where(h => h.ApplicationId == id)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        var profile = new ApplicationProfileDto
        {
            Id = application.Id,
            JobId = application.JobId,
            CandidateName = application.CandidateName,
            CandidateEmail = application.CandidateEmail,
            CoverLetter = application.CoverLetter,
            Stage = application.Stage,
            CultureFitScore = application.CultureFitScore,
            CultureFitComment = application.CultureFitComment,
            InterviewScore = application.InterviewScore,
            InterviewComment = application.InterviewComment,
            AssessmentScore = application.AssessmentScore,
            AssessmentComment = application.AssessmentComment,
            CreatedAt = application.CreatedAt,
            Notes = notes.Select(n => new NoteDto
            {
                Id = n.Id,
                Description = n.Description,
                Type = n.Type,
                AuthorName = n.CreatedByUser != null ? n.CreatedByUser.Name : "Unknown",
                CreatedAt = n.CreatedAt,
            }).ToList(),
            StageHistory = stageHistory.Select(h => new StageHistoryDto
            {
                FromStage = h.FromStage,
                ToStage = h.ToStage,
                ChangedBy = h.ChangedByUser != null ? h.ChangedByUser.Name : "Unknown",
                Reason = h.Reason,
                ChangedAt = h.ChangedAt,
            }).ToList(),
        };

        return Ok(profile);
    }

    // PATCH /api/applications/{id}/stage
    [HttpPatch("{id}/stage")]
    [TeamMemberHeaderFilter]
    public async Task<IActionResult> MoveStage(Guid id, [FromBody] MoveStageDto dto)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return NotFound($"Application with id '{id}' not found.");

        if (!StageTransitionValidator.IsValidTransition(application.Stage, dto.TargetStage, out var errorMessage))
            return Problem(errorMessage, statusCode: 400);

        var teamMemberId = Request.Headers["X-Team-Member-Id"].ToString();
        var teamMember = await _context.TeamMembers.FindAsync(Guid.Parse(teamMemberId));

        var stageHistory = new StageHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = id,
            FromStage = application.Stage,
            ToStage = dto.TargetStage,
            ChangedBy = Guid.Parse(teamMemberId),
            Reason = dto.Reason ?? string.Empty,
            ChangedAt = DateTime.UtcNow,
        };

        application.Stage = dto.TargetStage;

        _context.StageHistories.Add(stageHistory);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            application.Id,
            PreviousStage = stageHistory.FromStage,
            CurrentStage = stageHistory.ToStage,
            ChangedBy = teamMember != null ? teamMember.Name : "Unknown",
            ChangedAt = stageHistory.ChangedAt,
        });
    }
}
