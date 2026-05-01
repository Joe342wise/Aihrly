using System;
using Aihrly.Services;
using Xunit;

namespace Aihrly.Tests;

public class StageTransitionValidatorTests
{
    [Theory]
    [InlineData("applied", "screening")]
    [InlineData("applied", "rejected")]
    [InlineData("screening", "interview")]
    [InlineData("screening", "rejected")]
    [InlineData("interview", "offer")]
    [InlineData("interview", "rejected")]
    [InlineData("offer", "hired")]
    [InlineData("offer", "rejected")]
    public void ValidTransitions_ShouldReturnTrue(string from, string to)
    {
        var result = StageTransitionValidator.IsValidTransition(from, to, out _);
        Assert.True(result);
    }

    [Theory]
    [InlineData("applied", "hired")]
    [InlineData("screening", "hired")]
    [InlineData("interview", "hired")]
    [InlineData("hired", "screening")]
    [InlineData("rejected", "applied")]
    [InlineData("hired", "interview")]
    public void InvalidTransitions_ShouldReturnFalse(string from, string to)
    {
        var result = StageTransitionValidator.IsValidTransition(from, to, out var errorMessage);
        Assert.False(result);
        Assert.NotEmpty(errorMessage);
    }
}
