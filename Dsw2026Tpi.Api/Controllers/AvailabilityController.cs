using Dsw2026Tpi.Api.Configurations;
using Microsoft.AspNetCore.RateLimiting;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/availabilities")]
[Tags("04 - Availability")]
[Authorize(Policy = Policies.AdminPolicy)]
[EnableRateLimiting(RateLimitingConfiguration.GeneralPolicy)]
public class AvailabilityController : AppController
{
    private readonly IAvailabilityService _service;

    public AvailabilityController(IAvailabilityService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] AvailabilityModel.Request request)
    {
        var availability = await _service.Create(request);

        return Created(string.Empty, availability);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromBody] AvailabilityModel.Request request)
    {
        var availability = await _service.Update(request);

        return Ok(availability);
    }
}
