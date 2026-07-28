using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("patients")]
[Authorize(Policy = Policies.AdminPolicy)]
public class PatientController : AppController
{
    private readonly IPatientService _service;

    public PatientController(IPatientService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageSize,
        [FromQuery] int pageIndex,
        [FromQuery] string? name = null)
    {
        var patients = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(patients);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var patient = await _service.GetById(id);

        if (patient == null)
            return NotFound();

        return Ok(patient);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] PatientModel.Request request)
    {
        var patient = await _service.Update(id, request);

        return Ok(patient);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);

        return NoContent();
    }
}