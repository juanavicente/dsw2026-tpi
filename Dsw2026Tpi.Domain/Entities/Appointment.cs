using Dsw2026Tpi.Domain.Enums;

namespace Dsw2026Tpi.Domain.Entities;

public class Appointment : EntityBase
{
    public DateOnly AppointmentDate { get; private set; }

    public DateOnly? CancellationDate { get; private set; }

    public string Reason { get; private set; }

    public AppointmentStatus Status { get; private set; }

    public Guid TurnId { get; private set; }
    public Turn? Turn { get; private set; }

    public Guid PatientId { get; private set; }
    public Patient? Patient { get; private set; }

    #region Constructor for EF

#pragma warning disable CS8618
    private Appointment()
    {
    }
#pragma warning restore CS8618

    #endregion

    public Appointment(
        DateOnly appointmentDate,
        Guid turnId,
        Guid patientId,
        string reason,
        Guid? id = null)
        : base(id)
    {
        Validate(reason);

        AppointmentDate = appointmentDate;
        TurnId = turnId;
        PatientId = patientId;
        Reason = reason.Trim();

        Status = AppointmentStatus.Confirmed;
    }

    public void Cancel(DateOnly cancellationDate)
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new ArgumentException(
                "Solo una cita confirmada puede cancelarse.");

        Status = AppointmentStatus.Cancelled;
        CancellationDate = cancellationDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new ArgumentException(
                "Solo una cita confirmada puede completarse.");

        Status = AppointmentStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "El motivo es obligatorio.",
                nameof(reason));

        if (reason.Trim().Length < 5)
            throw new ArgumentException(
                "El motivo debe tener al menos 5 caracteres.",
                nameof(reason));

        if (reason.Trim().Length > 250)
            throw new ArgumentException(
                "El motivo no puede superar los 250 caracteres.",
                nameof(reason));
    }
}