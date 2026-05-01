using System;
using Aihrly.Data;
using Aihrly.Dtos;
using Aihrly.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aihrly.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class JobsController : ControllerBase
{
    private readonly AihrlyDbContext _context;

    public JobsController(AihrlyDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Problem("Title is required.", statusCode: 400);

        if (string.IsNullOrWhiteSpace(dto.Description))
            return Problem("Description is required.", statusCode: 400);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            Status = "open",
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        var result = new JobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Status = job.Status,
        };

        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> ListJobs([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.Jobs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(j => j.Status == status);

        var total = await query.CountAsync();
        var jobs = await query
            .OrderBy(j => j.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = jobs.Select(j => new JobDto
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description,
            Location = j.Location,
            Status = j.Status,
        }).ToList();

        return Ok(new
        {
            total,
            page,
            pageSize,
            items = dtos,
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob(Guid id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job == null)
            return NotFound($"Job with id '{id}' not found.");

        var dto = new JobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            Status = job.Status,
        };

        return Ok(dto);
    }
}
