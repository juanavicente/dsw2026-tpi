using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    /// <summary>
    /// Reserva un turno médico.
    /// </summary>
    Task<AppointmentModel.Response> Create(
        AppointmentModel.Request request);

    /// <summary>
    /// Obtiene los turnos activos de un paciente.
    /// </summary>
    Task<IEnumerable<AppointmentModel.PatientResponse>> GetPatientAppointments(
        long dni);

    /// <summary>
    /// Cancela una cita.
    /// </summary>
    Task Cancel(Guid id);
}