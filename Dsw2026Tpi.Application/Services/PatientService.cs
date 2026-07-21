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

    public async Task<Pagination<PatientModel.Response>> GetAll(int pageSize, int pageIndex)
    {
        throw new NotImplementedException();
    }
}