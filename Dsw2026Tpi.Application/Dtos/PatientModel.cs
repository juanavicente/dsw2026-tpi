using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record PatientModel
    {
        public record Request(
            string Name,
            string Dni,
            string Email,
            string Phone);

        public record Response(
            Guid Id,
            string Name,
            string Dni,
            string Email,
            string Phone);

        public record Login(
            string Email,
            string Dni);
    }
}