using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Helpers;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISignInService _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IPersistence _persistence;

    public AuthenticationService(UserManager<ApplicationUser> userManager,
        ISignInService signInManager,
        RoleManager<IdentityRole> roleManager,
        JwtService jwtService,
        ILogger<AuthenticationService> logger,
        IPersistence persistence)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _logger = logger;
        _persistence = persistence;

    }

    public async Task<LoginAdminModel.Response> LoginAdmin(LoginAdminModel.Request request)
    {
        if (!request.Email.IsEmailValid()) throw new AuthenticationException();
        var user = await _userManager.FindByEmailAsync(request.Email) ?? throw new AuthenticationException();
        var result = await _signInManager.CheckPassword(user, request.Password);

        if (!result)
        {
            _logger.LogError("Intento de login fallido para: {Email}", request.Email);
            throw new AuthenticationException();
        }

        //var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
        var role = Roles.Administrator;

        var token  = _jwtService.GenerateToken(user.UserName!, role);

        return new LoginAdminModel.Response(
            token,
            role
        );
    }

    public async Task<LoginPatientModel.Response> LoginPatient(LoginPatientModel.Request request)
    {
        if (!request.Email.IsEmailValid())
            throw new AuthenticationException();
        var dni = request.Dni.ToString();

        if (dni.Length < 7 || dni.Length > 8)
        {
            throw new ValidationException(
                ErrorCodes.REGISTER_USER_INVALID,
                nameof(ErrorCodes.REGISTER_USER_INVALID));
        }

        // Buscar si existe un paciente con ese DNI
        var patientByDni = await _persistence.First<Patient>(p =>
            p.Dni == request.Dni.ToString());

        // Si existe el DNI pero pertenece a otro email -> Conflicto
        if (patientByDni is not null && patientByDni.Email != request.Email)
        {
            throw new ConflictException(
                nameof(ErrorCodes.REGISTER_USER_CONFLICT),
                ErrorCodes.REGISTER_USER_CONFLICT);
        }

        // Buscar paciente por email + dni
        var patient = await _persistence.First<Patient>(p =>
            p.Email == request.Email &&
            p.Dni == request.Dni.ToString());

        if (patient is null)
        {
            patient = new Patient(
                name: string.Empty,
                dni: request.Dni.ToString(),
                email: request.Email,
                phone: string.Empty);

            await _persistence.Add(patient);

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
                throw new AuthenticationException();

            await _userManager.AddToRoleAsync(user, Roles.Patient);
        }

        var identityUser = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new AuthenticationException();

        var role = Roles.Patient;

        var token = _jwtService.GenerateToken(identityUser.UserName!, role);

        return new LoginPatientModel.Response(token, role);
    }

    public async Task<RegisterModel.Response> Register(RegisterModel.Request request)
    {
        if (!request.Email.IsEmailValid()) throw new ValidationException(ErrorCodes.REGISTER_USER_INVALID,
            nameof(ErrorCodes.REGISTER_USER_INVALID));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        //var result = await _userManager.CreateAsync(user, request.Password);

        //if (!result.Succeeded) throw new ConflictException(nameof(ErrorCodes.REGISTER_USER_CONFLICT),
        //ErrorCodes.REGISTER_USER_CONFLICT)
        //.WithDetail(result.Errors.Select(e => (e.Code, e.Description)));
        try
        {
            var result = await _userManager.CreateAsync(user, request.Password);


            if (!result.Succeeded)
            {
                var errores = string.Join("\n", result.Errors.Select(e =>
                    $"{e.Code} - {e.Description}"));

                throw new Exception(errores);
            }

            var roles = await _roleManager.Roles.ToListAsync();

            Console.WriteLine("Cantidad de roles: " + roles.Count);

            Console.WriteLine("ROLES:");

            foreach (var r in roles)
            {
                Console.WriteLine($"{r.Id} - {r.Name}");
            }

           // _ = await _userManager.AddToRoleAsync(user, Roles.Administrator);

            _logger.LogInformation("Usuario registrado: {Email}", request.Email);

            return new RegisterModel.Response(request.Email);
        }
        catch (Exception ex) { Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
