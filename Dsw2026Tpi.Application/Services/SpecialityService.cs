using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class SpecialityService : ISpecialityService
{
    private readonly IPersistence _persistence;

    public SpecialityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<SpecialityModel.Response>> GetAll(
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

        var specialities = await _persistence.Paginate<Speciality, string>(
            pageSize,
            pageIndex,
            s => string.IsNullOrWhiteSpace(name) ||
                 s.Name.Contains(name),
            s => s.Name);

        return specialities.Map(s =>
            new SpecialityModel.Response(
                s.Id,
                s.Name,
                s.Description));
    }

    public async Task<SpecialityModel.Response?> GetById(Guid id)
    {
        var speciality = await _persistence.GetById<Speciality>(id);

        if (speciality == null)
            return null;

        return new SpecialityModel.Response(
            speciality.Id,
            speciality.Name,
            speciality.Description);
    }

    public async Task<SpecialityModel.Response> Create(SpecialityModel.Request request)
    {
        try
        {
            var existing = await _persistence.First<Speciality>(
                s => s.Name.ToLower() == request.Name.Trim().ToLower());

            if (existing != null)
                throw new ConflictException(
                    "SPECIALITY_ALREADY_EXISTS",
                    "Ya existe una especialidad con ese nombre.");

            var speciality = new Speciality(
                request.Name,
                request.Description);

            await _persistence.Add(speciality);

            return new SpecialityModel.Response(
                speciality.Id,
                speciality.Name,
                speciality.Description);
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(
                ex.Message,
                nameof(ErrorCodes.VALIDATION_ERROR));
        }
    }

    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        try
        {
            var speciality = await _persistence.GetById<Speciality>(id);

            if (speciality == null)
                throw new EntityNotFoundException("Speciality");

            var duplicated = await _persistence.First<Speciality>(
                s => s.Name.ToLower() == request.Name.Trim().ToLower()
                     && s.Id != id);

            if (duplicated != null)
                throw new ConflictException(
                    "SPECIALITY_ALREADY_EXISTS",
                    "Ya existe una especialidad con ese nombre.");

            speciality.Update(
                request.Name,
                request.Description);

            await _persistence.Update(speciality);

            return new SpecialityModel.Response(
                speciality.Id,
                speciality.Name,
                speciality.Description);
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
        var speciality = await _persistence.GetById<Speciality>(id);

        if (speciality == null)
            throw new EntityNotFoundException("Speciality");

        await _persistence.Delete(speciality);
    }
}