namespace CicdDashboard.Api.Models;

public enum BuildStatus
{
    Queued,
    Running,
    Success,
    Failed,
    Canceled
}

public enum BuildSource
{
    GitHubActions,
    Jenkins
}

public record Build(
    string Id,
    BuildSource Source,
    string Repository,
    string Branch,
    string CommitSha,
    string CommitMessage,
    string Author,
    BuildStatus Status,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string LogsUrl
);

public record RetriggerResult(string NewBuildId, BuildSource Source, DateTime QueuedAt);
