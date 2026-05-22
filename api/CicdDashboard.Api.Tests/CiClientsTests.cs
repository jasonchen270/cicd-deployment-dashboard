using CicdDashboard.Api.CiClients;
using CicdDashboard.Api.Models;

namespace CicdDashboard.Api.Tests;

public class CiClientsTests
{
    [Fact]
    public async Task GitHubActionsClient_ReturnsSeededBuilds()
    {
        var client = new GitHubActionsClient();
        var builds = await client.ListRecentBuildsAsync();

        Assert.Equal(15, builds.Count);
        Assert.All(builds, b => Assert.Equal(BuildSource.GitHubActions, b.Source));
        Assert.All(builds, b => Assert.StartsWith("gha-", b.Id));
    }

    [Fact]
    public async Task JenkinsClient_ReturnsSeededBuilds()
    {
        var client = new JenkinsClient();
        var builds = await client.ListRecentBuildsAsync();

        Assert.Equal(10, builds.Count);
        Assert.All(builds, b => Assert.Equal(BuildSource.Jenkins, b.Source));
        Assert.All(builds, b => Assert.StartsWith("jenkins-", b.Id));
    }

    [Fact]
    public async Task GitHubActionsClient_Retrigger_ProducesNewId()
    {
        var client = new GitHubActionsClient();
        var result = await client.RetriggerAsync("gha-1000");

        Assert.StartsWith("gha-", result.NewBuildId);
        Assert.Equal(BuildSource.GitHubActions, result.Source);
    }

    [Fact]
    public async Task QueuedAndRunningBuilds_HaveNoFinishedAt()
    {
        var client = new GitHubActionsClient();
        var builds = await client.ListRecentBuildsAsync();

        foreach (var b in builds)
        {
            if (b.Status is BuildStatus.Queued or BuildStatus.Running)
                Assert.Null(b.FinishedAt);
            else
                Assert.NotNull(b.FinishedAt);
        }
    }
}
