using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record AvailabilityModel
{
    public record Request(
        Guid DoctorId,
        IEnumerable<DayRequest> Days);

    public record DayRequest(
        DayOfWeek Day,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public record Response(
        Guid Id,
        Guid DoctorId,
        int Year,
        int Month,
        DayOfWeek Day,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public record DoctorAvailabilityResponse(
        Guid Id,
        string Day,
        string StartTime,
        string EndTime);
}
