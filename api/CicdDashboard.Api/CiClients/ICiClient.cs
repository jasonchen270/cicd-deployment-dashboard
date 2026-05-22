using CicdDashboard.Api.Models;

namespace CicdDashboard.Api.CiClients;

public interface ICiClient
{
    BuildSource Source { get; }
    Task<IReadOnlyList<Build>> ListRecentBuildsAsync(CancellationToken ct = default);
    Task<RetriggerResult> RetriggerAsync(string buildId, CancellationToken ct = default);
}
