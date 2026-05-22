using CicdDashboard.Api.Models;

namespace CicdDashboard.Api.CiClients;

public sealed class GitHubActionsClient : ICiClient
{
    private static readonly string[] Repos =
        { "octo-org/payments-service", "octo-org/web-app", "octo-org/notifications" };
    private static readonly string[] Branches = { "main", "develop", "feature/checkout-v2", "hotfix/perms" };
    private static readonly string[] Authors = { "asmith", "bchen", "cwong", "dpatel" };

    public BuildSource Source => BuildSource.GitHubActions;

    public Task<IReadOnlyList<Build>> ListRecentBuildsAsync(CancellationToken ct = default)
    {
        var rng = new Random(7);
        var now = DateTime.UtcNow;
        var builds = Enumerable.Range(0, 15).Select(i =>
        {
            var status = (BuildStatus)rng.Next(0, 5);
            var started = now.AddMinutes(-i * 17 - rng.Next(0, 5));
            DateTime? finished = status is BuildStatus.Queued or BuildStatus.Running
                ? null
                : started.AddMinutes(rng.Next(2, 14));
            var sha = Guid.NewGuid().ToString("N")[..7];
            return new Build(
                Id: $"gha-{i + 1000}",
                Source: BuildSource.GitHubActions,
                Repository: Repos[i % Repos.Length],
                Branch: Branches[i % Branches.Length],
                CommitSha: sha,
                CommitMessage: SampleMessage(i),
                Author: Authors[i % Authors.Length],
                Status: status,
                StartedAt: started,
                FinishedAt: finished,
                LogsUrl: $"https://github.com/{Repos[i % Repos.Length]}/actions/runs/{i + 1000}"
            );
        }).ToList();
        return Task.FromResult<IReadOnlyList<Build>>(builds);
    }

    public Task<RetriggerResult> RetriggerAsync(string buildId, CancellationToken ct = default)
    {
        var newId = $"gha-{Random.Shared.Next(2000, 9999)}";
        return Task.FromResult(new RetriggerResult(newId, BuildSource.GitHubActions, DateTime.UtcNow));
    }

    private static string SampleMessage(int i) => (i % 5) switch
    {
        0 => "Refactor payment provider abstraction",
        1 => "Fix flaky integration test in checkout",
        2 => "Bump dependencies to address CVE-2024-1234",
        3 => "Add metrics for retry queue depth",
        _ => "Update README and deployment notes"
    };
}
