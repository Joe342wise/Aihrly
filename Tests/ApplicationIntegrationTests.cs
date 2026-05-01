using System.Net;
using System.Net.Http.Json;
using Aihrly.Dtos;
using Aihrly.Models;
using Aihrly.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aihrly.Tests;

public class ApplicationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApplicationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Test 1: Create application → add note → read back with author name
    [Fact]
    public async Task AddNote_ShouldReturnAuthorName()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AihrlyDbContext>();
        db.Database.EnsureCreated();

        var teamMember = await db.TeamMembers.FirstAsync();

        var job = new Job
        {
            Id = Guid.NewGuid(),
            Title = "Developer",
            Description = "Build things",
            Location = "Remote",
            Status = "open",
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        var applyResponse = await _client.PostAsJsonAsync(
            $"/api/jobs/{job.Id}/applications",
            new CreateApplicationDto { CandidateName = "Alice", CandidateEmail = "alice@test.com" });
        applyResponse.EnsureSuccessStatusCode();
        var application = await applyResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var noteRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/applications/{application!.Id}/notes");
        noteRequest.Headers.Add("X-Team-Member-Id", teamMember.Id.ToString());
        noteRequest.Content = JsonContent.Create(new CreateNoteDto { Type = "general", Description = "Good candidate" });
        var response = await _client.SendAsync(noteRequest);
        response.EnsureSuccessStatusCode();

        var profileResponse = await _client.GetAsync($"/api/applications/{application.Id}");
        profileResponse.EnsureSuccessStatusCode();
        var profile = await profileResponse.Content.ReadFromJsonAsync<ApplicationProfileDto>();

        Assert.NotNull(profile);
        Assert.Single(profile!.Notes);
        Assert.Equal("Sarah Chen", profile.Notes[0].AuthorName);
    }

    // Test 2: Submit score twice — second value wins
    [Fact]
    public async Task SubmitScoreTwice_SecondValueShouldWin()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AihrlyDbContext>();
        db.Database.EnsureCreated();

        var job = new Job { Id = Guid.NewGuid(), Title = "Dev", Description = "Desc", Location = "Remote", Status = "open" };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        var applyResponse = await _client.PostAsJsonAsync(
            $"/api/jobs/{job.Id}/applications",
            new CreateApplicationDto { CandidateName = "Bob", CandidateEmail = "bob@test.com" });
        applyResponse.EnsureSuccessStatusCode();
        var application = await applyResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        var teamMember = await db.TeamMembers.FirstAsync();
        var teamMemberId = teamMember.Id.ToString();

        var score1 = new HttpRequestMessage(HttpMethod.Put, $"/api/applications/{application!.Id}/scores/culture-fit");
        score1.Headers.Add("X-Team-Member-Id", teamMemberId);
        score1.Content = JsonContent.Create(new UpdateScoreDto { Score = 3, Comment = "First" });
        var response1 = await _client.SendAsync(score1);
        response1.EnsureSuccessStatusCode();

        var score2 = new HttpRequestMessage(HttpMethod.Put, $"/api/applications/{application.Id}/scores/culture-fit");
        score2.Headers.Add("X-Team-Member-Id", teamMemberId);
        score2.Content = JsonContent.Create(new UpdateScoreDto { Score = 5, Comment = "Second" });
        var response2 = await _client.SendAsync(score2);
        response2.EnsureSuccessStatusCode();

        var profile = await _client.GetFromJsonAsync<ApplicationProfileDto>($"/api/applications/{application.Id}");
        Assert.Equal(5, profile!.CultureFitScore);
        Assert.Equal("Second", profile.CultureFitComment);
    }

    // Test 3: Duplicate application returns 409
    [Fact]
    public async Task DuplicateApplication_ShouldReturnConflict()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AihrlyDbContext>();
        db.Database.EnsureCreated();

        var job = new Job { Id = Guid.NewGuid(), Title = "Dev", Description = "Desc", Location = "Remote", Status = "open" };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        var dto = new CreateApplicationDto { CandidateName = "Charlie", CandidateEmail = "charlie@test.com" };

        var response1 = await _client.PostAsJsonAsync($"/api/jobs/{job.Id}/applications", dto);
        response1.EnsureSuccessStatusCode();

        var response2 = await _client.PostAsJsonAsync($"/api/jobs/{job.Id}/applications", dto);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }
}
