using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IPatientService
{
    Task<Pagination<PatientModel.Response>> GetAll(
        int pageSize,
        int pageIndex,
        string? name = null);

    Task<PatientModel.Response?> GetById(Guid id);

    Task<Patient> Create(PatientModel.Request request);

    Task<PatientModel.Response> Update(Guid id, PatientModel.Request request);

    Task Delete(Guid id);
}