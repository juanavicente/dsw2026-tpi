using System;
using System.Collections.Generic;
using System.Text;

using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;


namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IPersistence _persistence;

    public AvailabilityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<AvailabilityModel.Response> Create(
        AvailabilityModel.Request request)
    {

        try
        {
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);

            if (doctor == null)
                throw new EntityNotFoundException("Doctor");

            Availability? lastAvailability = null;

            foreach (var day in request.Days)
            {
                if (day.StartTime >= day.EndTime)
                    throw new ValidationException(
                        "La hora de inicio debe ser menor a la hora de fin.",
                        nameof(ErrorCodes.VALIDATION_ERROR));

                var overlap = await _persistence.First<Availability>(
                    a =>
                        a.DoctorId == request.DoctorId &&
                        a.Year == DateTime.Today.Year &&
                        a.Month == DateTime.Today.Month &&
                        a.DayOfWeek == day.Day &&
                        day.StartTime < a.EndTime &&
                        day.EndTime > a.StartTime);

                if (overlap != null)
                    throw new ConflictException(
                        "AVAILABILITY_OVERLAP",
                        "Ya existe una disponibilidad que se superpone con ese horario.");

                var availability = new Availability(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    day.Day,
                    day.StartTime,
                    day.EndTime,
                    request.DoctorId);

                lastAvailability = await _persistence.Add(availability);
                var holidays = await _persistence.GetHolidays();

                var dates = GetDatesForMonth(day.Day)
                    .Where(date => !holidays.Contains(date))
                    .ToList();

                foreach (var date in dates)
                {
                    var turns = GenerateTurns(availability, date);

                    Console.WriteLine($"Fecha: {date} - Turnos generados: {turns.Count}");

                    await _persistence.AddRange(turns);
                }
            }

            return new AvailabilityModel.Response(
                lastAvailability!.Id,
                lastAvailability.DoctorId,
                lastAvailability.Year,
                lastAvailability.Month,
                lastAvailability.DayOfWeek,
                lastAvailability.StartTime,
                lastAvailability.EndTime);
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(
                ex.Message,
                nameof(ErrorCodes.VALIDATION_ERROR));
        }
    }

    public async Task<AvailabilityModel.Response> Update(
     AvailabilityModel.Request request)
    {
        var availabilities = await _persistence.GetFiltered<Availability>(
            a => a.DoctorId == request.DoctorId &&
                 a.Year == DateTime.Today.Year &&
                 a.Month == DateTime.Today.Month);

        if (availabilities != null)
        {
            foreach (var availability in availabilities)
            {
                await _persistence.Delete(availability);
            }
        }

        return await Create(request);
    }

    public async Task<IEnumerable<AvailabilityModel.DoctorAvailabilityResponse>> GetByDoctor(Guid doctorId)
    {
        var doctor = await _persistence.GetById<Doctor>(doctorId);

        if (doctor == null)
            throw new EntityNotFoundException("Doctor");

        var availabilities = await _persistence.GetFiltered<Availability>(
            a => a.DoctorId == doctorId &&
                 a.Year == DateTime.Today.Year &&
                 a.Month == DateTime.Today.Month);

        return availabilities.Select(a =>
            new AvailabilityModel.DoctorAvailabilityResponse(
                a.Id,
                a.DayOfWeek.ToString(),
                a.StartTime.ToString("HH:mm"),
                a.EndTime.ToString("HH:mm")
            ));
    }

    private List<DateOnly> GetDatesForMonth(DayOfWeek dayOfWeek)
    {
        var dates = new List<DateOnly>();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var current = today;

        var lastDay = new DateOnly(
            today.Year,
            today.Month,
            DateTime.DaysInMonth(today.Year, today.Month));

        while (current <= lastDay)
        {
            if (current.DayOfWeek == dayOfWeek)
            {
                dates.Add(current);
            }

            current = current.AddDays(1);
        }

        return dates;
    }

    private List<Turn> GenerateTurns(
        Availability availability,
        DateOnly date)
    {
        var turns = new List<Turn>();

        var current = availability.StartTime;

        while (current < availability.EndTime)
        {
            var end = current.AddMinutes(30);

            if (end > availability.EndTime)
                break;

            turns.Add(new Turn(
                date,
                current,
                end,
                availability.Id));

            current = end;
        }

        return turns;
    }
}
