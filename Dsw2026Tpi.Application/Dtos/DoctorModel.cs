namespace Dsw2026Tpi.Application.Dtos;

public record DoctorModel
{
    public record Request(
        string Name,
        string LicenseNumber,
        Guid SpecialityId);

    public record SpecialityResponse(
        Guid Id,
        string Name);

    public record Response(
        Guid Id,
        string Name,
        string LicenseNumber,
        SpecialityResponse Speciality);
}
