using CicdDashboard.Api.Models;

namespace CicdDashboard.Api.CiClients;

public sealed class JenkinsClient : ICiClient
{
    private static readonly string[] Jobs =
        { "nightly-e2e", "release-pipeline", "infra-terraform-plan" };
    private static readonly string[] Branches = { "main", "release/2026-q2" };
    private static readonly string[] Authors = { "ekim", "fortega", "ghassan" };

    public BuildSource Source => BuildSource.Jenkins;

    public Task<IReadOnlyList<Build>> ListRecentBuildsAsync(CancellationToken ct = default)
    {
        var rng = new Random(13);
        var now = DateTime.UtcNow;
        var builds = Enumerable.Range(0, 10).Select(i =>
        {
            var status = (BuildStatus)rng.Next(0, 5);
            var started = now.AddMinutes(-i * 23 - rng.Next(0, 5));
            DateTime? finished = status is BuildStatus.Queued or BuildStatus.Running
                ? null
                : started.AddMinutes(rng.Next(5, 32));
            return new Build(
                Id: $"jenkins-{i + 500}",
                Source: BuildSource.Jenkins,
                Repository: Jobs[i % Jobs.Length],
                Branch: Branches[i % Branches.Length],
                CommitSha: Guid.NewGuid().ToString("N")[..7],
                CommitMessage: SampleMessage(i),
                Author: Authors[i % Authors.Length],
                Status: status,
                StartedAt: started,
                FinishedAt: finished,
                LogsUrl: $"https://jenkins.internal/job/{Jobs[i % Jobs.Length]}/{i + 500}/console"
            );
        }).ToList();
        return Task.FromResult<IReadOnlyList<Build>>(builds);
    }

    public Task<RetriggerResult> RetriggerAsync(string buildId, CancellationToken ct = default)
    {
        var newId = $"jenkins-{Random.Shared.Next(600, 999)}";
        return Task.FromResult(new RetriggerResult(newId, BuildSource.Jenkins, DateTime.UtcNow));
    }

    private static string SampleMessage(int i) => (i % 4) switch
    {
        0 => "Run nightly cross-browser suite",
        1 => "Cut release candidate 2026.05.0",
        2 => "Terraform plan for staging VPC",
        _ => "Smoke test post-deploy"
    };
}
