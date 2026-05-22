using CicdDashboard.Api.CiClients;
using CicdDashboard.Api.Models;
using CicdDashboard.Api.Services;

namespace CicdDashboard.Api.Tests;

public class BuildAggregatorTests
{
    private sealed class StubClient : ICiClient
    {
        public BuildSource Source { get; }
        private readonly IReadOnlyList<Build> _builds;
        public bool RetriggerCalled { get; private set; }

        public StubClient(BuildSource source, IReadOnlyList<Build> builds)
        {
            Source = source;
            _builds = builds;
        }

        public Task<IReadOnlyList<Build>> ListRecentBuildsAsync(CancellationToken ct = default)
            => Task.FromResult(_builds);

        public Task<RetriggerResult> RetriggerAsync(string buildId, CancellationToken ct = default)
        {
            RetriggerCalled = true;
            return Task.FromResult(new RetriggerResult("new-" + buildId, Source, DateTime.UtcNow));
        }
    }

    private static Build MakeBuild(string id, BuildSource src, DateTime started) => new(
        Id: id,
        Source: src,
        Repository: "repo",
        Branch: "main",
        CommitSha: "abc1234",
        CommitMessage: "msg",
        Author: "alice",
        Status: BuildStatus.Success,
        StartedAt: started,
        FinishedAt: started.AddMinutes(5),
        LogsUrl: "https://example.test/logs"
    );

    [Fact]
    public async Task GetAllAsync_MergesAndSortsByStartedAtDescending()
    {
        var t0 = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var gh = new StubClient(BuildSource.GitHubActions, new[]
        {
            MakeBuild("gha-1", BuildSource.GitHubActions, t0),
            MakeBuild("gha-2", BuildSource.GitHubActions, t0.AddMinutes(10)),
        });
        var jk = new StubClient(BuildSource.Jenkins, new[]
        {
            MakeBuild("jenkins-1", BuildSource.Jenkins, t0.AddMinutes(5)),
        });
        var agg = new BuildAggregator(new ICiClient[] { gh, jk });

        var result = await agg.GetAllAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal("gha-2", result[0].Id);
        Assert.Equal("jenkins-1", result[1].Id);
        Assert.Equal("gha-1", result[2].Id);
    }

    [Theory]
    [InlineData("gha-123", BuildSource.GitHubActions)]
    [InlineData("jenkins-99", BuildSource.Jenkins)]
    [InlineData("GHA-7", BuildSource.GitHubActions)]
    public void ParseSource_RecognizesPrefixes(string id, BuildSource expected)
    {
        Assert.Equal(expected, BuildAggregator.ParseSource(id));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("unknown-1")]
    public void ParseSource_ReturnsNullForUnknown(string? id)
    {
        Assert.Null(BuildAggregator.ParseSource(id!));
    }

    [Fact]
    public async Task RetriggerAsync_RoutesToCorrectClient()
    {
        var gh = new StubClient(BuildSource.GitHubActions, Array.Empty<Build>());
        var jk = new StubClient(BuildSource.Jenkins, Array.Empty<Build>());
        var agg = new BuildAggregator(new ICiClient[] { gh, jk });

        var result = await agg.RetriggerAsync("jenkins-42");

        Assert.True(jk.RetriggerCalled);
        Assert.False(gh.RetriggerCalled);
        Assert.Equal(BuildSource.Jenkins, result.Source);
    }

    [Fact]
    public async Task RetriggerAsync_ThrowsOnUnknownPrefix()
    {
        var agg = new BuildAggregator(new ICiClient[] { new StubClient(BuildSource.GitHubActions, Array.Empty<Build>()) });
        await Assert.ThrowsAsync<ArgumentException>(() => agg.RetriggerAsync("mystery-1"));
    }
}
