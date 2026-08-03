using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class Patient : EntityBase

{

    public string Name { get; init; }

    public string Dni { get; init; }

    public string Email { get; init; }

    public string Phone { get; init; }

    #region Constructor for EF

#pragma warning disable CS8618

    private Patient()

    {

    }

#pragma warning restore CS8618

    #endregion

    public Patient(

        string name,

        string dni,

        string email,

        string phone,

        Guid? id = null) : base(id)

    {

        Name = name;

        Dni = dni;

        Email = email;

        Phone = phone;

    }
}