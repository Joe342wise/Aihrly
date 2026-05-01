using System;
using Aihrly.Data;
using Aihrly.Dtos;
using Aihrly.Filters;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Aihrly.Controllers;

[ApiController]
[Route("api/applications/{id}/scores")]
public class ScoresController : ControllerBase
{
    private readonly AihrlyDbContext _context;
    private readonly IDatabase? _cache;

    public ScoresController(AihrlyDbContext context, IConnectionMultiplexer? redis = null)
    {
        _context = context;
        _cache = redis?.GetDatabase();
    }

    // PUT /api/applications/{id}/scores/culture-fit
    [HttpPut("culture-fit")]
    [TeamMemberHeaderFilter]
    public async Task<IActionResult> UpdateCultureFit(Guid id, [FromBody] UpdateScoreDto dto)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return Problem(title: "Not Found", detail: $"Application with id '{id}' not found.", statusCode: 404);

        if (dto.Score < 1 || dto.Score > 5)
            return Problem(title: "Validation Error", detail: "Score must be between 1 and 5.", statusCode: 400);

        var teamMemberId = Guid.Parse(Request.Headers["X-Team-Member-Id"].ToString());

        application.CultureFitScore = dto.Score;
        application.CultureFitComment = dto.Comment;
        application.CultureFitUpdatedBy = teamMemberId;
        application.CultureFitUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (_cache != null)
        {
            await _cache.KeyDeleteAsync($"application:{id}");
        }

        return Ok(new
        {
            dimension = "culture-fit",
            score = application.CultureFitScore,
            comment = application.CultureFitComment,
            updatedBy = teamMemberId,
            updatedAt = application.CultureFitUpdatedAt,
        });
    }

    // PUT /api/applications/{id}/scores/interview
    [HttpPut("interview")]
    [TeamMemberHeaderFilter]
    public async Task<IActionResult> UpdateInterview(Guid id, [FromBody] UpdateScoreDto dto)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return Problem(title: "Not Found", detail: $"Application with id '{id}' not found.", statusCode: 404);

        if (dto.Score < 1 || dto.Score > 5)
            return Problem(title: "Validation Error", detail: "Score must be between 1 and 5.", statusCode: 400);

        var teamMemberId = Guid.Parse(Request.Headers["X-Team-Member-Id"].ToString());

        application.InterviewScore = dto.Score;
        application.InterviewComment = dto.Comment;
        application.InterviewUpdatedBy = teamMemberId;
        application.InterviewUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (_cache != null)
        {
            await _cache.KeyDeleteAsync($"application:{id}");
        }

        return Ok(new
        {
            dimension = "interview",
            score = application.InterviewScore,
            comment = application.InterviewComment,
            updatedBy = teamMemberId,
            updatedAt = application.InterviewUpdatedAt,
        });
    }

    // PUT /api/applications/{id}/scores/assessment
    [HttpPut("assessment")]
    [TeamMemberHeaderFilter]
    public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdateScoreDto dto)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return Problem(title: "Not Found", detail: $"Application with id '{id}' not found.", statusCode: 404);

        if (dto.Score < 1 || dto.Score > 5)
            return Problem(title: "Validation Error", detail: "Score must be between 1 and 5.", statusCode: 400);

        var teamMemberId = Guid.Parse(Request.Headers["X-Team-Member-Id"].ToString());

        application.AssessmentScore = dto.Score;
        application.AssessmentComment = dto.Comment;
        application.AssessmentUpdatedBy = teamMemberId;
        application.AssessmentUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (_cache != null)
        {
            await _cache.KeyDeleteAsync($"application:{id}");
        }

        return Ok(new
        {
            dimension = "assessment",
            score = application.AssessmentScore,
            comment = application.AssessmentComment,
            updatedBy = teamMemberId,
            updatedAt = application.AssessmentUpdatedAt,
        });
    }
}
