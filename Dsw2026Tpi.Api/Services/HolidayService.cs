using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;

namespace Dsw2026Tpi.Api.Services;

public class HolidayService
{
    private readonly IWebHostEnvironment _environment;

    public HolidayService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public List<DateOnly> GetHolidays()
    {
        var path = Path.Combine(
            _environment.ContentRootPath,
            "Resources",
            "holidays.json");

        if (!File.Exists(path))
            return new List<DateOnly>();

        var json = File.ReadAllText(path);

        var dates = JsonSerializer.Deserialize<List<DateOnly>>(json);

        return dates ?? new List<DateOnly>();
    }
}
