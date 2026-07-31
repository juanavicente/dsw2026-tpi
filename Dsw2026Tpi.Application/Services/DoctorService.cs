using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(
        int pageSize,
        int pageIndex,
        string? name = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Trim().Length < 3 || name.Trim().Length > 100)
                throw new ValidationException(
                    "El filtro por nombre debe tener entre 3 y 100 caracteres.",
                    nameof(ErrorCodes.VALIDATION_ERROR));
        }

        var doctors = await _persistence.Paginate<Doctor, string>(
            pageSize,
            pageIndex,
            d => string.IsNullOrWhiteSpace(name) ||
                 d.Name.Contains(name),
            d => d.Name,
            nameof(Doctor.Speciality));

        return doctors.Map(d =>
            new DoctorModel.Response(
                d.Id,
                d.Name,
                d.LicenseNumber,
                new DoctorModel.SpecialityResponse(
                    d.Speciality!.Id,
                    d.Speciality.Name)));
    }

    public async Task<DoctorModel.Response?> GetById(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(
            id,
            nameof(Doctor.Speciality));

        if (doctor == null)
            return null;

        return new DoctorModel.Response(
            doctor.Id,
            doctor.Name,
            doctor.LicenseNumber,
            new DoctorModel.SpecialityResponse(
                doctor.Speciality!.Id,
                doctor.Speciality.Name));
    }

    public async Task<DoctorModel.Response> Create(
        DoctorModel.Request request)
    {
        try
        {
            var speciality = await _persistence.GetById<Speciality>(
                request.SpecialityId);

            if (speciality == null)
                throw new EntityNotFoundException("Speciality");

            var doctor = new Doctor(
                request.Name,
                request.LicenseNumber,
                speciality);

            await _persistence.Add(doctor);

            return new DoctorModel.Response(
                doctor.Id,
                doctor.Name,
                doctor.LicenseNumber,
                new DoctorModel.SpecialityResponse(
                    speciality.Id,
                    speciality.Name));
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(
                ex.Message,
                nameof(ErrorCodes.VALIDATION_ERROR));
        }
    }

    public async Task<DoctorModel.Response> Update(
        Guid id,
        DoctorModel.Request request)
    {
        try
        {
            var doctor = await _persistence.GetById<Doctor>(
                id,
                nameof(Doctor.Speciality));

            if (doctor == null)
                throw new EntityNotFoundException("Doctor");

            var speciality = await _persistence.GetById<Speciality>(
                request.SpecialityId);

            if (speciality == null)
                throw new EntityNotFoundException("Speciality");

            doctor.Update(
                request.Name,
                request.LicenseNumber,
                speciality);

            await _persistence.Update(doctor);

            return new DoctorModel.Response(
                doctor.Id,
                doctor.Name,
                doctor.LicenseNumber,
                new DoctorModel.SpecialityResponse(
                    speciality.Id,
                    speciality.Name));
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(
                ex.Message,
                nameof(ErrorCodes.VALIDATION_ERROR));
        }
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);

        if (doctor == null)
            throw new EntityNotFoundException("Doctor");

        await _persistence.Delete(doctor);
    }
}
