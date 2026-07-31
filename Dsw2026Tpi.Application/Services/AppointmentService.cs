using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;

    public AppointmentService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<AppointmentModel.Response> Create(
        AppointmentModel.Request request)
    {
        try
        {
            // Validación del DNI
            var dni = request.Patient.Dni.ToString();

            if (dni.Length < 7 || dni.Length > 10)
                throw new ValidationException(
                    "El DNI debe tener entre 7 y 10 dígitos.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Validación del motivo
            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ValidationException(
                    "El motivo es obligatorio.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            if (request.Reason.Trim().Length < 5)
                throw new ValidationException(
                    "El motivo debe tener al menos 5 caracteres.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Buscar turno
            var turn = await _persistence.GetById<Turn>(request.TurnId);

            if (turn == null)
                throw new EntityNotFoundException("Turn");

            // Validar Availability
            var availability = await _persistence.GetById<Availability>(turn.AvailabilityId);

            if (availability == null)
                throw new EntityNotFoundException("Availability");

            // Validar Doctor
            var doctor = await _persistence.GetById<Doctor>(availability.DoctorId);

            if (doctor == null)
                throw new EntityNotFoundException("Doctor");

            // Validar disponibilidad
            if (turn.Status != TurnStatus.Available)
                throw new ConflictException(
                    "APPOINTMENT_CONFLICT",
                    "El turno seleccionado ya no se encuentra disponible.");

            // No permitir fechas pasadas
            if (turn.Date < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ValidationException(
                    "No es posible reservar turnos en fechas pasadas.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Buscar paciente
            var patient = await _persistence.First<Patient>(
                p => p.Dni == dni);

            if (patient == null)
                throw new EntityNotFoundException("Patient");

            // Verificar doble reserva
            var appointmentExists =
                await _persistence.First<Appointment>(
                    a => a.TurnId == turn.Id
                      && a.Status == AppointmentStatus.Confirmed);

            if (appointmentExists != null)
                throw new ConflictException(
                    "APPOINTMENT_CONFLICT",
                    "El turno ya fue reservado por otro paciente.");

            // Reservar turno
            turn.Reserve();

            await _persistence.Update(turn);

            // Crear cita
            var appointment = new Appointment(
                turn.Date,
                turn.Id,
                patient.Id,
                request.Reason);

            await _persistence.Add(appointment);

            return new AppointmentModel.Response(
                appointment.Id,
                appointment.AppointmentDate,
                appointment.CancellationDate,
                appointment.TurnId,
                appointment.PatientId,
                appointment.Reason,
                appointment.Status.ToString());
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(
                ex.Message,
                nameof(ErrorCodes.VALIDATION_ERROR));
        }
    }

    public async Task<IEnumerable<AppointmentModel.PatientResponse>>
        GetPatientAppointments(long dni)
    {
        // Validar DNI
        var patientDni = dni.ToString();

        if (patientDni.Length < 7 || patientDni.Length > 10)
            throw new ValidationException(
                "El DNI debe tener entre 7 y 10 dígitos.",
                nameof(ErrorCodes.VALIDATION_ERROR));

        // Buscar paciente
        var patient = await _persistence.First<Patient>(
            p => p.Dni == patientDni);

        if (patient == null)
            throw new EntityNotFoundException("Patient");

        // Obtener únicamente citas activas
        var appointments = await _persistence.GetFiltered<Appointment>(
            a => a.PatientId == patient.Id
                 && a.Status == AppointmentStatus.Confirmed);

        if (appointments == null || !appointments.Any())
            return Enumerable.Empty<AppointmentModel.PatientResponse>();

        var response = new List<AppointmentModel.PatientResponse>();

        foreach (var appointment in appointments)
        {
            var turn = await _persistence.GetById<Turn>(
                appointment.TurnId,
                $"{nameof(Turn.Availability)}.{nameof(Availability.Doctor)}");

            if (turn == null)
                continue;

            if (turn.Availability == null)
                continue;

            if (turn.Availability.Doctor == null)
                continue;

            response.Add(
                new AppointmentModel.PatientResponse(
                    appointment.Id,
                    appointment.AppointmentDate,
                    turn.StartTime,
                    turn.EndTime,
                    turn.Availability.Doctor.Name,
                    turn.Availability.Doctor.Speciality?.Name ?? string.Empty,
                    appointment.Reason,
                    appointment.Status.ToString()));
        }

        return response;
    }

    public async Task Cancel(Guid id)
    {
        try
        {
            var appointment = await _persistence.GetById<Appointment>(
                id);

            if (appointment == null)
                throw new EntityNotFoundException("Appointment");

            if (appointment.Status != AppointmentStatus.Confirmed)
                throw new ValidationException(
                    "Solo pueden cancelarse citas confirmadas.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            var turn = await _persistence.GetById<Turn>(
                appointment.TurnId);

            if (turn == null)
                throw new EntityNotFoundException("Turn");

            appointment.Cancel(
                DateOnly.FromDateTime(DateTime.UtcNow));

            turn.Release();

            await _persistence.Update(turn);

            await _persistence.Update(appointment);
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(
                ex.Message,
                nameof(ErrorCodes.VALIDATION_ERROR));
        }
    }
}

