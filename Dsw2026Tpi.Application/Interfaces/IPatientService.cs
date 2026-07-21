using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Dtos.Patient;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IPatientService
{
    Task<Pagination<PatientModel.Response>> GetAll(
        int pageSize,
        int pageIndex,
        string? name = null);

    Task<PatientModel.Response?> GetById(Guid id);

    Task<PatientModel.Response> Create(PatientModel.Request request);

    Task<PatientModel.Response> Update(Guid id, PatientModel.Request request);

    Task Delete(Guid id);
}
