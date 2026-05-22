using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CicdDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CicdDashboard.Api.Tests;

public class BuildsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public BuildsControllerIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Get_Builds_Returns200WithMergedList()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/builds");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var builds = await response.Content.ReadFromJsonAsync<List<Build>>(Json);
        Assert.NotNull(builds);
        Assert.Equal(25, builds!.Count);
    }

    [Fact]
    public async Task Get_Builds_FilterByStatus_NarrowsResults()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/builds?status=Failed");
        response.EnsureSuccessStatusCode();
        var builds = await response.Content.ReadFromJsonAsync<List<Build>>(Json);

        Assert.NotNull(builds);
        Assert.All(builds!, b => Assert.Equal(BuildStatus.Failed, b.Status));
    }

    [Fact]
    public async Task Post_Retrigger_ReturnsAccepted()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/builds/gha-1000/retrigger", content: null);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RetriggerResult>(Json);
        Assert.NotNull(result);
        Assert.Equal(BuildSource.GitHubActions, result!.Source);
    }

    [Fact]
    public async Task Post_Retrigger_BadIdReturns400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/builds/mystery-1/retrigger", content: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Health_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
