using CicdDashboard.Api.CiClients;
using CicdDashboard.Api.Models;

namespace CicdDashboard.Api.Services;

public sealed class BuildAggregator
{
    private readonly IEnumerable<ICiClient> _clients;

    public BuildAggregator(IEnumerable<ICiClient> clients) => _clients = clients;

    public async Task<IReadOnlyList<Build>> GetAllAsync(CancellationToken ct = default)
    {
        var tasks = _clients.Select(c => c.ListRecentBuildsAsync(ct));
        var results = await Task.WhenAll(tasks);
        return results
            .SelectMany(r => r)
            .OrderByDescending(b => b.StartedAt)
            .ToList();
    }

    public async Task<RetriggerResult> RetriggerAsync(string buildId, CancellationToken ct = default)
    {
        var source = ParseSource(buildId)
            ?? throw new ArgumentException($"Unrecognized build id: {buildId}", nameof(buildId));
        var client = _clients.FirstOrDefault(c => c.Source == source)
            ?? throw new InvalidOperationException($"No client registered for source {source}");
        return await client.RetriggerAsync(buildId, ct);
    }

    internal static BuildSource? ParseSource(string buildId)
    {
        if (string.IsNullOrWhiteSpace(buildId)) return null;
        if (buildId.StartsWith("gha-", StringComparison.OrdinalIgnoreCase)) return BuildSource.GitHubActions;
        if (buildId.StartsWith("jenkins-", StringComparison.OrdinalIgnoreCase)) return BuildSource.Jenkins;
        return null;
    }
}
