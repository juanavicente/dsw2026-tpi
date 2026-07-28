using System;
using System.Linq;

namespace Dsw2026Tpi.Domain.Entities;

public class Patient : EntityBase
{
    public string Name { get; private set; }
    public string Dni { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Patient() { }
#pragma warning restore CS8618
    #endregion

    // Constructor utilizado por el CRUD
    public Patient(
        string name,
        string dni,
        string email,
        string phone,
        Guid? id = null) : base(id)
    {
        Validate(name, dni, email, phone);

        Name = name.Trim();
        Dni = dni.Trim();
        Email = email.Trim().ToLower();
        Phone = phone.Trim();
    }

   
    public void Update(
        string name,
        string dni,
        string email,
        string phone)
    {
        Validate(name, dni, email, phone);

        Name = name.Trim();
        Dni = dni.Trim();
        Email = email.Trim().ToLower();
        Phone = phone.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(
        string name,
        string dni,
        string email,
        string phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre es obligatorio.", nameof(name));

        if (name.Trim().Length < 3 || name.Trim().Length > 100)
            throw new ArgumentException("El nombre debe tener entre 3 y 100 caracteres.", nameof(name));

        if (string.IsNullOrWhiteSpace(dni))
            throw new ArgumentException("El DNI es obligatorio.", nameof(dni));

        if (dni.Length < 7 || dni.Length > 8)
            throw new ArgumentException("El DNI debe tener 7 u 8 dígitos.", nameof(dni));

        if (!dni.All(char.IsDigit))
            throw new ArgumentException("El DNI solo puede contener números.", nameof(dni));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es obligatorio.", nameof(email));

        if (!email.Contains("@"))
            throw new ArgumentException("El email no tiene un formato válido.", nameof(email));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("El teléfono es obligatorio.", nameof(phone));
    }
}