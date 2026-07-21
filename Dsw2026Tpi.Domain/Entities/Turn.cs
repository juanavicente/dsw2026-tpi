using System;
using System.Collections.Generic;
using System.Text;

using Dsw2026Tpi.Domain.Enums;

namespace Dsw2026Tpi.Domain.Entities;

public class Turn : EntityBase
{
    public DateOnly Date { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }

    public TurnStatus Status { get; private set; }

    public Guid AvailabilityId { get; private set; }
    public Availability? Availability { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Turn()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Turn(
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid availabilityId,
        Guid? id = null) : base(id)
    {
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        AvailabilityId = availabilityId;
        Status = TurnStatus.Available;
    }

    public void Reserve()
    {
        Status = TurnStatus.Reserved;
    }

    public void Release()
    {
        Status = TurnStatus.Available;
    }

    public void Block()
    {
        Status = TurnStatus.Blocked;
    }
}
