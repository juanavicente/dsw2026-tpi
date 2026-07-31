using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Availability : EntityBase
{
    public int Year { get; private set; }

    public int Month { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public Guid DoctorId { get; private set; }

    public Doctor? Doctor { get; private set; }
    public ICollection<Turn> Turns { get; private set; } = [];

    #region Constructor for EF
#pragma warning disable CS8618
    private Availability()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Availability(
        int year,
        int month,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid doctorId,
        Guid? id = null) : base(id)
    {
        Validate(year, month, startTime, endTime);

        Year = year;
        Month = month;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        DoctorId = doctorId;
    }

    public void Update(
        int year,
        int month,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        Validate(year, month, startTime, endTime);

        Year = year;
        Month = month;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;

        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(
        int year,
        int month,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        if (year < DateTime.UtcNow.Year)
            throw new ArgumentException(
                "El año no puede ser menor al actual.",
                nameof(year));

        if (month < 1 || month > 12)
            throw new ArgumentException(
                "El mes debe estar entre 1 y 12.",
                nameof(month));

        if (startTime >= endTime)
            throw new ArgumentException(
                "La hora de inicio debe ser menor que la hora de fin.",
                nameof(startTime));
    }
}