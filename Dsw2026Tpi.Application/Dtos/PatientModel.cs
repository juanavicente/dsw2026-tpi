using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record PatientModel
    {
        public record Request(
            string FirstName,
            string LastName,
            string Dni,
            string Email,
            string Phone);

        public record Response(
            Guid Id,
            string FirstName,
            string LastName,
            string Dni,
            string Email,
            string Phone);

        public record Login(string Dni);
    }
}
}
