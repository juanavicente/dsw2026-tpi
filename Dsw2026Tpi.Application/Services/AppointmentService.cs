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
            // Validar DNI
            var dni = request.Patient.Dni.ToString();

            if (dni.Length < 7 || dni.Length > 10)
                throw new ValidationException(
                    "El DNI debe tener entre 7 y 10 dígitos.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Validar motivo
            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ValidationException(
                    "El motivo es obligatorio.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            if (request.Reason.Trim().Length < 5)
                throw new ValidationException(
                    "El motivo debe tener al menos 5 caracteres.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            if (request.Reason.Trim().Length > 250)
                throw new ValidationException(
                    "El motivo no puede superar los 250 caracteres.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Validar DoctorId
            if (request.DoctorId == Guid.Empty)
                throw new ValidationException(
                    "El médico es obligatorio.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Validar AvailabilitySlotId
            if (request.AvailabilitySlotId == Guid.Empty)
                throw new ValidationException(
                    "El turno es obligatorio.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Buscar médico
            var doctor = await _persistence.GetById<Doctor>(
                request.DoctorId);

            if (doctor == null)
                throw new EntityNotFoundException("Doctor");

            // Buscar slot de disponibilidad.
            // En nuestro dominio AvailabilitySlot está representado por Turn.
            var turn = await _persistence.GetById<Turn>(
                request.AvailabilitySlotId);

            if (turn == null)
                throw new EntityNotFoundException("Turn");

            // Buscar disponibilidad asociada al turno
            var availability = await _persistence.GetById<Availability>(
                turn.AvailabilityId);

            if (availability == null)
                throw new EntityNotFoundException("Availability");

            // Validar que el slot corresponda al médico enviado
            if (availability.DoctorId != doctor.Id)
                throw new ValidationException(
                    "El turno seleccionado no corresponde al médico indicado.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Validar que el turno esté disponible
            if (turn.Status != TurnStatus.Available)
                throw new ConflictException(
                    "APPOINTMENT_CONFLICT",
                    "El turno seleccionado ya no se encuentra disponible.");

            // No permitir fechas u horarios pasados
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);

            if (turn.Date < currentDate ||
                (turn.Date == currentDate && turn.StartTime <= currentTime))
            {
                throw new ValidationException(
                    "No es posible reservar turnos en fechas u horarios pasados.",
                    nameof(ErrorCodes.VALIDATION_ERROR));
            }

            // Buscar paciente
            var patient = await _persistence.First<Patient>(
                p => p.Dni == dni);

            if (patient == null)
                throw new EntityNotFoundException("Patient");

            // Verificar que no exista una reserva activa para el mismo slot
            Appointment? appointment = null;

            await _persistence.ExecuteInTransaction(async () =>
            {
                // Volver a obtener el turno dentro de la transacción
                var currentTurn = await _persistence.GetById<Turn>(
                    request.AvailabilitySlotId);

                if (currentTurn == null)
                    throw new EntityNotFoundException("Turn");

                // Verificar nuevamente el estado dentro de la transacción
                if (currentTurn.Status != TurnStatus.Available)
                    throw new ConflictException(
                        "APPOINTMENT_CONFLICT",
                        "El turno seleccionado ya no se encuentra disponible.");

                // Verificar nuevamente que no exista una reserva activa
                var appointmentExists =
                    await _persistence.First<Appointment>(
                        a => a.TurnId == currentTurn.Id
                          && a.Status == AppointmentStatus.Booked);

                if (appointmentExists != null)
                    throw new ConflictException(
                        "APPOINTMENT_CONFLICT",
                        "El turno ya fue reservado por otro paciente.");

                // Reservar turno
                currentTurn.Reserve();

                await _persistence.Update(currentTurn);

                // Crear cita
                appointment = new Appointment(
                    currentTurn.Date,
                    currentTurn.Id,
                    patient.Id,
                    request.Reason);

                await _persistence.Add(appointment);
            });

            if (appointment == null)
                throw new InvalidOperationException(
                    "No fue posible crear la cita.");

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

    public async Task<IEnumerable<AppointmentModel.SearchResponse>> GetByDate(
    DateOnly date)
    {
        // Validar fecha
        if (date == default)
            throw new ValidationException(
                "La fecha es obligatoria.",
                nameof(ErrorCodes.VALIDATION_ERROR));

        // Obtener las citas de la fecha indicada
        var appointments = await _persistence.GetFiltered<Appointment>(
            a => a.AppointmentDate == date);

        if (appointments == null || !appointments.Any())
            return Enumerable.Empty<AppointmentModel.SearchResponse>();

        var response =
            new List<AppointmentModel.SearchResponse>();

        foreach (var appointment in appointments)
        {
            // Obtener paciente
            var patient = await _persistence.GetById<Patient>(
                appointment.PatientId);

            if (patient == null)
                continue;

            // Obtener turno
            var turn = await _persistence.GetById<Turn>(
                appointment.TurnId);

            if (turn == null)
                continue;

            // Obtener disponibilidad
            var availability = await _persistence.GetById<Availability>(
                turn.AvailabilityId);

            if (availability == null)
                continue;

            // Obtener médico con especialidad
            var doctor = await _persistence.GetById<Doctor>(
                availability.DoctorId,
                nameof(Doctor.Speciality));

            if (doctor == null)
                continue;

            if (doctor.Speciality == null ||
                doctor.SpecialityId == null)
                continue;

            if (!long.TryParse(patient.Dni, out var patientDni))
                continue;

            response.Add(
                new AppointmentModel.SearchResponse(
                    appointment.Id,
                    appointment.Status.ToString(),
                    new AppointmentModel.SearchPatientResponse(
                        patientDni,
                        patient.Name),
                    new AppointmentModel.SearchDoctorResponse(
                        doctor.Id,
                        doctor.Name,
                        new AppointmentModel.SearchSpecialityResponse(
                            doctor.SpecialityId.Value,
                            doctor.Speciality.Name))));
        }

        return response;
    }

    public async Task<Pagination<AppointmentModel.SearchResponse>> Search(
    int pageSize,
    int pageIndex,
    Guid? specialtyId = null,
    Guid? doctorId = null,
    long? dni = null,
    DateOnly? date = null)
    {
        // Validar paginación
        if (pageSize <= 0)
            throw new ValidationException(
                "El tamaño de página debe ser mayor a cero.",
                nameof(ErrorCodes.VALIDATION_ERROR));

        if (pageIndex < 0)
            throw new ValidationException(
                "El índice de página no puede ser negativo.",
                nameof(ErrorCodes.VALIDATION_ERROR));

        // Validar DNI únicamente si fue enviado
        string? patientDni = null;

        if (dni.HasValue)
        {
            patientDni = dni.Value.ToString();

            if (patientDni.Length < 7 || patientDni.Length > 10)
                throw new ValidationException(
                    "El DNI debe tener entre 7 y 10 dígitos.",
                    nameof(ErrorCodes.VALIDATION_ERROR));
        }

        // Validar especialidad únicamente si fue enviada
        if (specialtyId.HasValue)
        {
            if (specialtyId.Value == Guid.Empty)
                throw new ValidationException(
                    "La especialidad indicada no es válida.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            var speciality = await _persistence.GetById<Speciality>(
                specialtyId.Value);

            if (speciality == null)
                throw new EntityNotFoundException("Speciality");
        }

        // Validar médico únicamente si fue enviado
        if (doctorId.HasValue)
        {
            if (doctorId.Value == Guid.Empty)
                throw new ValidationException(
                    "El médico indicado no es válido.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            var doctor = await _persistence.GetById<Doctor>(
                doctorId.Value);

            if (doctor == null)
                throw new EntityNotFoundException("Doctor");
        }

        // Obtener página de citas aplicando todos los filtros opcionales.
        var appointments = await _persistence.Paginate<Appointment, DateOnly>(
            pageSize,
            pageIndex,
            a =>
                (!date.HasValue ||
                 a.AppointmentDate == date.Value)

                &&

                (!dni.HasValue ||
                 a.Patient!.Dni == patientDni)

                &&

                (!doctorId.HasValue ||
                 a.Turn!.Availability!.DoctorId == doctorId.Value)

                &&

                (!specialtyId.HasValue ||
                 a.Turn!.Availability!.Doctor!.SpecialityId == specialtyId.Value),

            a => a.AppointmentDate,

            nameof(Appointment.Patient),
            nameof(Appointment.Turn),
            $"{nameof(Appointment.Turn)}.{nameof(Turn.Availability)}",
            $"{nameof(Appointment.Turn)}.{nameof(Turn.Availability)}.{nameof(Availability.Doctor)}",
            $"{nameof(Appointment.Turn)}.{nameof(Turn.Availability)}.{nameof(Availability.Doctor)}.{nameof(Doctor.Speciality)}");

        var response =
            new List<AppointmentModel.SearchResponse>();

        foreach (var appointment in appointments.Data)
        {
            var patient = appointment.Patient;
            var turn = appointment.Turn;
            var availability = turn?.Availability;
            var doctor = availability?.Doctor;
            var speciality = doctor?.Speciality;

            if (patient == null ||
                turn == null ||
                availability == null ||
                doctor == null ||
                speciality == null ||
                doctor.SpecialityId == null)
            {
                continue;
            }

            if (!long.TryParse(patient.Dni, out var parsedDni))
                continue;

            response.Add(
                new AppointmentModel.SearchResponse(
                    appointment.Id,
                    appointment.Status.ToString(),
                    new AppointmentModel.SearchPatientResponse(
                        parsedDni,
                        patient.Name),
                    new AppointmentModel.SearchDoctorResponse(
                        doctor.Id,
                        doctor.Name,
                        new AppointmentModel.SearchSpecialityResponse(
                            doctor.SpecialityId.Value,
                            speciality.Name))));
        }

        return new Pagination<AppointmentModel.SearchResponse>(
            appointments.PageSize,
            appointments.PageIndex,
            appointments.Total,
            response);
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
                 && a.Status == AppointmentStatus.Booked);

        if (appointments == null || !appointments.Any())
            return Enumerable.Empty<AppointmentModel.PatientResponse>();

        var response =
            new List<AppointmentModel.PatientResponse>();

        foreach (var appointment in appointments)
        {
            // Buscar Turn
            var turn = await _persistence.GetById<Turn>(
                appointment.TurnId);

            if (turn == null)
                continue;

            // Buscar Availability
            var availability =
                await _persistence.GetById<Availability>(
                    turn.AvailabilityId);

            if (availability == null)
                continue;

            // Buscar Doctor incluyendo Speciality
            var doctor = await _persistence.GetById<Doctor>(
                availability.DoctorId,
                nameof(Doctor.Speciality));

            if (doctor == null)
                continue;

            response.Add(
                new AppointmentModel.PatientResponse(
                    appointment.Id,
                    appointment.AppointmentDate,
                    turn.StartTime,
                    turn.EndTime,
                    doctor.Name,
                    doctor.Speciality?.Name ?? string.Empty,
                    appointment.Reason,
                    appointment.Status.ToString()));
        }

        return response;
    }

    public async Task Cancel(Guid id)
    {
        try
        {
            // Buscar cita
            var appointment =
                await _persistence.GetById<Appointment>(id);

            if (appointment == null)
                throw new EntityNotFoundException("Appointment");

            // Solo una cita activa puede cancelarse
            if (appointment.Status != AppointmentStatus.Booked)
                throw new ValidationException(
                    "Solo pueden cancelarse citas reservadas.",
                    nameof(ErrorCodes.VALIDATION_ERROR));

            // Buscar Turn
            var turn = await _persistence.GetById<Turn>(
                appointment.TurnId);

            if (turn == null)
                throw new EntityNotFoundException("Turn");

            // Cancelar cita
            appointment.Cancel(
                DateOnly.FromDateTime(DateTime.UtcNow));

            // Liberar slot
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