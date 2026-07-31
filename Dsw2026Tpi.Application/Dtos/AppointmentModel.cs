namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record PatientRequest(
        long Dni);

    public record Request(
        Guid AvailabilitySlotId,
        PatientRequest Patient,
        string Reason);

    public record Response(
        Guid Id,
        DateOnly AppointmentDate,
        DateOnly? CancellationDate,
        Guid AvailabilitySlotId,
        Guid PatientId,
        string Reason,
        string Status);

    public record PatientResponse(
        Guid Id,
        DateOnly AppointmentDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Doctor,
        string Speciality,
        string Reason,
        string Status);

    public record SearchResponse(
        Guid Id,
        string Speciality,
        string Doctor,
        long Dni,
        DateOnly AppointmentDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Status);
}