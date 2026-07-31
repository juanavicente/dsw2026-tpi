using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("apiappointments")]
[Authorize]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController(
        IAppointmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Reserva un turno.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] AppointmentModel.Request request)
    {
        var appointment = await _service.Create(request);

        return CreatedAtAction(
            nameof(GetPatientAppointments),
            new
            {
                dni = request.Patient.Dni
            },
            appointment);
    }

    /// <summary>
    /// Obtiene las citas activas del paciente.
    /// </summary>
    [HttpGet("patient")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientAppointments(
        [FromQuery] long dni)
    {
        var appointments =
            await _service.GetPatientAppointments(dni);

        return Ok(appointments);
    }

    /// <summary>
    /// Cancela una cita.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        Guid id)
    {
        await _service.Cancel(id);

        return NoContent();
    }
}