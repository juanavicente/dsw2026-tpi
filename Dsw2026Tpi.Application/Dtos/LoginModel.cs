using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Application.Dtos;

public record LoginAdminModel
{
    public record Request(
        [Required]
        [EmailAddress]
        string Email,

        [Required]
        [MinLength(8)]
        string Password
    );

    public record Response(string? Token, string? Role);
}

public record LoginPatientModel
{
    public record Request(
        [Required]
        [EmailAddress]
        string Email,

        [Range(1000000, 99999999)]
        long Dni
    );

    public record Response(string? Token, string? Role);
}