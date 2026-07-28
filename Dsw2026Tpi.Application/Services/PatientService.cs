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

    public async Task<Patient> Create(PatientModel.Request request)
    {
        var patient = new Patient(
            request.Name,
            request.Dni,
            request.Email,
            request.Phone);

        await _persistence.Add(patient);

        return patient;
    }

    public async Task<PatientModel.Response> Update(Guid id, PatientModel.Request request)
    {
        var patient = await _persistence.GetById<Patient>(id);

        if (patient == null)
            throw new ArgumentException("El paciente no existe.");

        var duplicatedDni = await _persistence.First<Patient>(
            p => p.Dni == request.Dni && p.Id != id);

        if (duplicatedDni != null)
            throw new ArgumentException("Ya existe un paciente con ese DNI.");

        var duplicatedEmail = await _persistence.First<Patient>(
            p => p.Email.ToLower() == request.Email.Trim().ToLower()
                 && p.Id != id);

        if (duplicatedEmail != null)
            throw new ArgumentException("Ya existe un paciente con ese email.");

        patient.Update(
            request.Name,
            request.Dni,
            request.Email,
            request.Phone);

        await _persistence.Update(patient);

        return new PatientModel.Response(
            patient.Id,
            patient.Name,
            patient.Dni,
            patient.Email,
            patient.Phone);
    }

    public async Task Delete(Guid id)
    {
        var patient = await _persistence.GetById<Patient>(id);

        if (patient == null)
            throw new ArgumentException("El paciente no existe.");

        await _persistence.Delete(patient);
    }
}