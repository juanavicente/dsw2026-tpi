namespace Dsw2026Tpi.Domain.Entities;

public class Speciality : EntityBase
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Speciality() { }
#pragma warning restore CS8618
    #endregion

    public Speciality(string name, string description, Guid? id = null) : base(id)
    {
        Validate(name, description);

        Name = name.Trim();
        Description = description.Trim();
    }

    public void Update(string name, string description)
    {
        Validate(name, description);

        Name = name.Trim();
        Description = description.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre es obligatorio.", nameof(name));

        if (name.Trim().Length < 3 || name.Trim().Length > 100)
            throw new ArgumentException("El nombre debe tener entre 3 y 100 caracteres.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("La descripción es obligatoria.", nameof(description));

        if (description.Trim().Length < 10 || description.Trim().Length > 100)
            throw new ArgumentException("La descripción debe tener entre 10 y 100 caracteres.", nameof(description));
    }
}