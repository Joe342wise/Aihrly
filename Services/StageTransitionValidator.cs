using System;

namespace Aihrly.Services;

public static class StageTransitionValidator
{
    private static readonly Dictionary<string, string[]> ValidTransitions = new()
    {
        { "applied", new[] { "screening", "rejected" } },
        { "screening", new[] { "interview", "rejected" } },
        { "interview", new[] { "offer", "rejected" } },
        { "offer", new[] { "hired", "rejected" } },
    };

    private static readonly HashSet<string> TerminalStages = new() { "hired", "rejected" };

    public static bool IsValidTransition(string fromStage, string toStage, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (TerminalStages.Contains(fromStage))
        {
            errorMessage = $"Cannot transition from '{fromStage}' — it is a terminal stage.";
            return false;
        }

        if (!ValidTransitions.TryGetValue(fromStage, out var allowed))
        {
            errorMessage = $"Unknown stage: '{fromStage}'.";
            return false;
        }

        if (!allowed.Contains(toStage))
        {
            errorMessage = $"Invalid transition from '{fromStage}' to '{toStage}'. Allowed transitions: {string.Join(", ", allowed)}.";
            return false;
        }

        return true;
    }
}
