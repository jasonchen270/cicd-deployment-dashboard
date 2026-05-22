using CicdDashboard.Api.Models;
using CicdDashboard.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CicdDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BuildsController : ControllerBase
{
    private readonly BuildAggregator _aggregator;

    public BuildsController(BuildAggregator aggregator) => _aggregator = aggregator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Build>>> List(
        [FromQuery] BuildStatus? status,
        [FromQuery] BuildSource? source,
        CancellationToken ct)
    {
        var builds = await _aggregator.GetAllAsync(ct);
        if (status is not null) builds = builds.Where(b => b.Status == status).ToList();
        if (source is not null) builds = builds.Where(b => b.Source == source).ToList();
        return Ok(builds);
    }

    [HttpPost("{id}/retrigger")]
    public async Task<ActionResult<RetriggerResult>> Retrigger(string id, CancellationToken ct)
    {
        try
        {
            var result = await _aggregator.RetriggerAsync(id, ct);
            return Accepted(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
