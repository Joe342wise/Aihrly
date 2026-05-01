using System;
using Aihrly.Data;
using Aihrly.Dtos;
using Aihrly.Filters;
using Aihrly.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Aihrly.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly AihrlyDbContext _context;
    private readonly IDatabase? _cache;

    public NotesController(AihrlyDbContext context, IConnectionMultiplexer? redis = null)
    {
        _context = context;
        _cache = redis?.GetDatabase();
    }

    // POST /api/applications/{id}/notes
    [HttpPost("/api/applications/{id}/notes")]
    [TeamMemberHeaderFilter]
    public async Task<IActionResult> AddNote(Guid id, [FromBody] CreateNoteDto dto)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return Problem(title: "Not Found", detail: $"Application with id '{id}' not found.", statusCode: 404);

        var validTypes = new[] { "general", "screening", "interview", "reference_check", "red_flag" };
        if (!validTypes.Contains(dto.Type))
            return Problem(title: "Validation Error", detail: $"Invalid note type '{dto.Type}'. Valid types: {string.Join(", ", validTypes)}.", statusCode: 400);
        if (string.IsNullOrWhiteSpace(dto.Description))
            return Problem(title: "Validation Error", detail: "Description is required.", statusCode: 400);

        var teamMemberId = Request.Headers["X-Team-Member-Id"].ToString();

        var note = new ApplicationNote
        {
            Id = Guid.NewGuid(),
            ApplicationId = id,
            Description = dto.Description,
            Type = dto.Type,
            CreatedBy = Guid.Parse(teamMemberId),
            CreatedAt = DateTime.UtcNow,
        };

        _context.ApplicationNotes.Add(note);
        await _context.SaveChangesAsync();

        if (_cache != null)
        {
            await _cache.KeyDeleteAsync($"application:{id}");
        }

        var teamMember = await _context.TeamMembers.FindAsync(Guid.Parse(teamMemberId));

        var result = new NoteDto
        {
            Id = note.Id,
            Description = note.Description,
            Type = note.Type,
            AuthorName = teamMember != null ? teamMember.Name : "Unknown",
            CreatedAt = note.CreatedAt,
        };

        return CreatedAtAction(nameof(GetNotes), new { id = note.ApplicationId }, result);
    }

    // GET /api/applications/{id}/notes
    [HttpGet("/api/applications/{id}/notes")]
    public async Task<IActionResult> GetNotes(Guid id)
    {
        var application = await _context.Applications.FindAsync(id);
        if (application == null)
            return Problem(title: "Not Found", detail: $"Application with id '{id}' not found.", statusCode: 404);

        var notes = await _context.ApplicationNotes
            .Include(n => n.CreatedByUser)
            .Where(n => n.ApplicationId == id)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var dtos = notes.Select(n => new NoteDto
        {
            Id = n.Id,
            Description = n.Description,
            Type = n.Type,
            AuthorName = n.CreatedByUser != null ? n.CreatedByUser.Name : "Unknown",
            CreatedAt = n.CreatedAt,
        }).ToList();

        return Ok(dtos);
    }
}
