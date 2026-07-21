using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPersistence _persistence;

    public PatientService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<PatientModel.Response>> GetAll(
        int pageSize,
        int pageIndex,
        string? name = null)
    {
        var patients = await _persistence.Paginate<Patient, string>(
            pageSize,
            pageIndex,
            p => string.IsNullOrWhiteSpace(name) || p.Name.Contains(name),
            p => p.Name);

        return patients.Map(p =>
            new PatientModel.Response(
                p.Id,
                p.Name,
                p.Dni,
                p.Email,
                p.Phone));
    }

    public async Task<PatientModel.Response?> GetById(Guid id)
    {
        var patient = await _persistence.GetById<Patient>(id);

        if (patient == null)
            return null;

        return new PatientModel.Response(
            patient.Id,
            patient.Name,
            patient.Dni,
            patient.Email,
            patient.Phone);
    }

    public Task<PatientModel.Response> Create(PatientModel.Request request)
    {
        throw new NotImplementedException();
    }

    public Task<PatientModel.Response> Update(Guid id, PatientModel.Request request)
    {
        throw new NotImplementedException();
    }

    public Task Delete(Guid id)
    {
        throw new NotImplementedException();
    }
}