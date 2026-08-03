namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    /// <summary>
    /// Datos del paciente necesarios para solicitar una cita.
    /// </summary>
    public record PatientRequest(
        long Dni);

    /// <summary>
    /// Datos necesarios para reservar una cita.
    /// </summary>
    public record Request(
        Guid DoctorId,
        Guid AvailabilitySlotId,
        PatientRequest Patient,
        string Reason);

    /// <summary>
    /// Respuesta al crear una cita.
    /// </summary>
    public record Response(
        Guid Id,
        DateOnly AppointmentDate,
        DateOnly? CancellationDate,
        Guid AvailabilitySlotId,
        Guid PatientId,
        string Reason,
        string Status);

    /// <summary>
    /// Cita activa mostrada al paciente.
    /// </summary>
    public record PatientResponse(
        Guid Id,
        DateOnly AppointmentDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Doctor,
        string Speciality,
        string Reason,
        string Status);

    /// <summary>
    /// Paciente incluido en una búsqueda administrativa.
    /// </summary>
    public record SearchPatientResponse(
        long Dni,
        string FullName);

    /// <summary>
    /// Especialidad incluida en los datos del médico.
    /// </summary>
    public record SearchSpecialityResponse(
        Guid SpecialtyId,
        string Name);

    /// <summary>
    /// Médico incluido en una búsqueda administrativa.
    /// </summary>
    public record SearchDoctorResponse(
        Guid DoctorId,
        string Name,
        SearchSpecialityResponse Specialty);

    /// <summary>
    /// Elemento de una búsqueda administrativa de citas.
    /// </summary>
    public record SearchResponse(
        Guid AppointmentsId,
        string AppointmentsStatus,
        SearchPatientResponse Patient,
        SearchDoctorResponse Doctor);
}