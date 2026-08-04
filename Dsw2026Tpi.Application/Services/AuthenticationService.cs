using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISignInService _signInManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IPersistence _persistence;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        ISignInService signInManager,
        JwtService jwtService,
        ILogger<AuthenticationService> logger,
        IPersistence persistence)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
        _persistence = persistence;
    }
    public async Task<LoginAdminModel.Response> LoginAdmin(LoginAdminModel.Request request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email) ?? throw new AuthenticationException();
        var result = await _signInManager.CheckPassword(user, request.Password);

        if (!result)
        {
            _logger.LogWarning(
                "Intento de login fallido para: {Email}",
                request.Email);

            throw new AuthenticationException();
        }

        var isAdministrator = await _userManager.IsInRoleAsync(
            user,
            Roles.Administrator);

        if (!isAdministrator)
        {
            _logger.LogWarning(
                "Intento de acceso al login de administrador sin rol válido para: {Email}",
                request.Email);

            throw new AuthenticationException();
        }

        var role = Roles.Administrator;

        var token = _jwtService.GenerateToken(user.UserName!, role);

        return new LoginAdminModel.Response(
            token,
            role
        );
    }

    public async Task<LoginPatientModel.Response> LoginPatient(
        LoginPatientModel.Request request)
    {
        var dni = request.Dni.ToString();

        var patientByDni = await _persistence.First<Patient>(p =>
            p.Dni == dni);

        // Si existe el DNI pero pertenece a otro email -> Conflicto
        if (patientByDni is not null && patientByDni.Email != request.Email)
        {
            throw new ConflictException(
                nameof(ErrorCodes.REGISTER_USER_CONFLICT),
                ErrorCodes.REGISTER_USER_CONFLICT);
        }

        var patientByEmail = await _persistence.First<Patient>(p =>
            p.Email == request.Email);

        if (patientByEmail is not null && patientByEmail.Dni != dni)
        {
            throw new ConflictException(
                nameof(ErrorCodes.REGISTER_USER_CONFLICT),
                ErrorCodes.REGISTER_USER_CONFLICT);
        }

        var patient = await _persistence.First<Patient>(p =>
            p.Email == request.Email &&
            p.Dni == dni);

        // Crear el paciente si es su primer acceso
        if (patient is null)
        {
            patient = new Patient(
                name: string.Empty,
                dni: dni,
                email: request.Email,
                phone: string.Empty);

            await _persistence.Add(patient);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                throw new ConflictException(
                    nameof(ErrorCodes.REGISTER_USER_CONFLICT),
                    ErrorCodes.REGISTER_USER_CONFLICT)
                    .WithDetail(
                        result.Errors.Select(e => (e.Code, e.Description)));
            }
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Patient))
        {
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                Roles.Patient);

            if (!roleResult.Succeeded)
            {
                throw new ConflictException(
                    nameof(ErrorCodes.REGISTER_USER_CONFLICT),
                    ErrorCodes.REGISTER_USER_CONFLICT)
                    .WithDetail(
                        roleResult.Errors.Select(e => (e.Code, e.Description)));
            }
        }

        var role = Roles.Patient;

        var token = _jwtService.GenerateToken(
            user.UserName!,
            role);

        return new LoginPatientModel.Response(token, role);
    }
}