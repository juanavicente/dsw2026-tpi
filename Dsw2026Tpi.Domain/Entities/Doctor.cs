namespace Dsw2026Tpi.Domain.Entities;

public class Doctor : EntityBase
{
    public string Name { get; private set; }
    public string LicenseNumber { get; private set; }

    public bool IsActive { get; private set; }

    public Guid? SpecialityId { get; private set; }
    public Speciality? Speciality { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor() { }
#pragma warning restore CS8618
    #endregion

    public Doctor(
        string name,
        string licenseNumber,
        Speciality speciality,
        Guid? id = null)
        : base(id)
    {
        Validate(name, licenseNumber, speciality);

        Name = name.Trim();
        LicenseNumber = licenseNumber.Trim();

        Speciality = speciality;
        SpecialityId = speciality.Id;

        IsActive = true;
    }

    public void Update(
        string name,
        string licenseNumber,
        Speciality speciality)
    {
        Validate(name, licenseNumber, speciality);

        Name = name.Trim();
        LicenseNumber = licenseNumber.Trim();

        Speciality = speciality;
        SpecialityId = speciality.Id;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(
        string name,
        string licenseNumber,
        Speciality speciality)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "El nombre es obligatorio.",
                nameof(name));

        if (name.Trim().Length < 3 || name.Trim().Length > 100)
            throw new ArgumentException(
                "El nombre debe tener entre 3 y 100 caracteres.",
                nameof(name));

        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException(
                "La matrícula es obligatoria.",
                nameof(licenseNumber));

        if (speciality is null)
            throw new ArgumentException(
                "La especialidad es obligatoria.",
                nameof(speciality));
    }
}
