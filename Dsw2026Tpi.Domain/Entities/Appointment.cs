using System;
using System.Collections.Generic;
using System.Text;

using Dsw2026Tpi.Domain.Enums;

namespace Dsw2026Tpi.Domain.Entities;

public class Appointment : EntityBase
{
    public DateOnly AppointmentDate { get; private set; }
    public DateOnly? CancellationDate { get; private set; }

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
        Guid? id = null) : base(id)
    {
        AppointmentDate = appointmentDate;
        TurnId = turnId;
        PatientId = patientId;
        Status = AppointmentStatus.Confirmed;
    }

    public void Cancel(DateOnly cancellationDate)
    {
        Status = AppointmentStatus.Cancelled;
        CancellationDate = cancellationDate;
    }

    public void Complete()
    {
        Status = AppointmentStatus.Completed;
    }
}