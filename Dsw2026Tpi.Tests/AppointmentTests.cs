using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;

namespace Dsw2026Tpi.Tests;

public class AppointmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAppointmentAsBooked()
    {
        // Arrange
        var appointmentDate = new DateOnly(2026, 8, 10);
        var turnId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var reason = "Consulta médica general";

        // Act
        var appointment = new Appointment(
            appointmentDate,
            turnId,
            patientId,
            reason);

        // Assert
        Assert.Equal(appointmentDate, appointment.AppointmentDate);
        Assert.Equal(turnId, appointment.TurnId);
        Assert.Equal(patientId, appointment.PatientId);
        Assert.Equal(reason, appointment.Reason);
        Assert.Equal(AppointmentStatus.Booked, appointment.Status);
        Assert.Null(appointment.CancellationDate);
    }

    [Fact]
    public void Cancel_WhenAppointmentIsBooked_ShouldSetStatusToCancelled()
    {
        // Arrange
        var appointment = new Appointment(
            new DateOnly(2026, 8, 10),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Consulta médica general");

        var cancellationDate = new DateOnly(2026, 8, 3);

        // Act
        appointment.Cancel(cancellationDate);

        // Assert
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Equal(cancellationDate, appointment.CancellationDate);
    }

    [Fact]
    public void Cancel_WhenAppointmentIsAlreadyCancelled_ShouldThrowArgumentException()
    {
        // Arrange
        var appointment = new Appointment(
            new DateOnly(2026, 8, 10),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Consulta médica general");

        var cancellationDate = new DateOnly(2026, 8, 3);

        appointment.Cancel(cancellationDate);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            appointment.Cancel(cancellationDate));

        // Assert
        Assert.Equal(
            "Solo una cita reservada puede cancelarse.",
            exception.Message);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Create_WithReasonShorterThanFiveCharacters_ShouldThrowArgumentException()
    {
        // Arrange
        var appointmentDate = new DateOnly(2026, 8, 10);
        var turnId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var reason = "Dol";

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new Appointment(
                appointmentDate,
                turnId,
                patientId,
                reason));

        // Assert
        Assert.Contains(
            "El motivo debe tener al menos 5 caracteres.",
            exception.Message);
    }
}