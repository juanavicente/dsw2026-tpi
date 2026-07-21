using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Availability : EntityBase

{

    public int Year { get; init; }

    public int Month { get; init; }

    public DayOfWeek DayOfWeek { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    public Guid DoctorId { get; private set; }

    public Doctor? Doctor { get; private set; }

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

        Year = year;

        Month = month;

        DayOfWeek = dayOfWeek;

        StartTime = startTime;

        EndTime = endTime;

        DoctorId = doctorId;

    }
}