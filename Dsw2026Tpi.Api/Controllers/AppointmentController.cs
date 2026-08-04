using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dsw2026Tpi.Api.Configurations;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/appointments")]
[Tags("05 - Appointment")]
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
    /// Reserva un turno médico.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitingConfiguration.AppointmentBookingPolicy)]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] AppointmentModel.Request request)
    {
        var appointment = await _service.Create(request);

        return StatusCode(
            StatusCodes.Status201Created,
            appointment);
    }

    /// <summary>
    /// Obtiene las citas de una fecha determinada.
    /// </summary>
    [HttpGet]
    [EnableRateLimiting(RateLimitingConfiguration.GeneralPolicy)]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByDate(
        [FromQuery] DateOnly date)
    {
        var appointments =
            await _service.GetByDate(date);

        return Ok(appointments);
    }

    /// <summary>
    /// Realiza una búsqueda administrativa de citas
    /// aplicando filtros opcionales y paginación.
    /// </summary>
    [HttpGet("search")]
    [EnableRateLimiting(RateLimitingConfiguration.GeneralPolicy)]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Search(
        [FromQuery] int pageSize,
        [FromQuery] int pageIndex,
        [FromQuery] Guid? specialtyId = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] long? dni = null,
        [FromQuery] DateOnly? date = null)
    {
        var appointments = await _service.Search(
            pageSize,
            pageIndex,
            specialtyId,
            doctorId,
            dni,
            date);

        return Ok(appointments);
    }

    /// <summary>
    /// Obtiene las citas activas de un paciente.
    /// </summary>
    [HttpGet("patient")]
    [EnableRateLimiting(RateLimitingConfiguration.GeneralPolicy)]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [EnableRateLimiting(RateLimitingConfiguration.GeneralPolicy)]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _service.Cancel(id);

        return Ok("ok");
    }
}