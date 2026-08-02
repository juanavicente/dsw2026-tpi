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
    /// Obtiene las citas de una fecha determinada.
    /// </summary>
    Task<IEnumerable<AppointmentModel.SearchResponse>> GetByDate(
        DateOnly date);

    /// <summary>
    /// Realiza una búsqueda administrativa de citas
    /// aplicando filtros opcionales y paginación.
    /// </summary>
    Task<Pagination<AppointmentModel.SearchResponse>> Search(
        int pageSize,
        int pageIndex,
        Guid? specialtyId = null,
        Guid? doctorId = null,
        long? dni = null,
        DateOnly? date = null);

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