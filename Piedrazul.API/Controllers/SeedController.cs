using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Enums;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.API.Controllers;

/// <summary>
/// Endpoints temporales para datos de prueba.
/// ELIMINAR en producción.
/// </summary>
[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDoctorRepository _doctorRepository;

    public SeedController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IDoctorRepository doctorRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _doctorRepository = doctorRepository;
    }

    /// <summary>POST /api/seed/admin — Crea el usuario admin si no existe</summary>
    [HttpPost("admin")]
    public async Task<IActionResult> CreateAdmin()
    {
        if (await _userRepository.UsernameExistsAsync("admin"))
            return Conflict(new { message = "El usuario 'admin' ya existe." });

        var user = Domain.Entities.User.Create(
            "admin",
            _passwordHasher.Hash("admin123"),
            "Administrador",
            UserRole.Admin,
            "admin@piedrazul.com");

        await _userRepository.AddAsync(user);

        return Ok(new { message = "Usuario admin creado.", username = "admin", password = "admin123" });
    }

    /// <summary>POST /api/seed/doctor — Crea un doctor de prueba con horarios Lun-Vie 08:00-17:00</summary>
    [HttpPost("doctor")]
    public async Task<IActionResult> CreateDoctor()
    {
        var doctor = Doctor.Create(
            "Dr. Carlos Pérez",
            DoctorType.Doctor,
            Specialty.NeuralTherapy,
            30);

        await _doctorRepository.AddAsync(doctor);

        // Crear horarios Lun-Vie 08:00-17:00 automáticamente
        var workDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                               DayOfWeek.Thursday, DayOfWeek.Friday };
        foreach (var day in workDays)
        {
            var schedule = DoctorSchedule.Create(doctor.Id, day, TimeSpan.FromHours(8), TimeSpan.FromHours(17));
            doctor.AddSchedule(schedule);
            await _doctorRepository.AddScheduleAsync(schedule);
        }

        return Ok(new
        {
            message = "Doctor creado con horarios Lun-Vie 08:00-17:00.",
            doctorId = doctor.Id,
            fullName = doctor.FullName,
            specialty = doctor.Specialty.ToString(),
            intervalMinutes = doctor.AppointmentIntervalMinutes,
            scheduledDays = workDays.Select(d => d.ToString()).ToArray()
        });
    }

    /// <summary>
    /// POST /api/seed/schedules/{doctorId} — Asigna horarios Lun-Vie 08:00-17:00 a un doctor existente.
    /// Omite días que ya tengan horario configurado.
    /// </summary>
    [HttpPost("schedules/{doctorId:guid}")]
    public async Task<IActionResult> SeedSchedules(Guid doctorId)
    {
        var doctor = await _doctorRepository.GetByIdWithSchedulesAsync(doctorId);
        if (doctor is null)
            return NotFound(new { message = $"No se encontró médico con id {doctorId}." });

        var workDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                               DayOfWeek.Thursday, DayOfWeek.Friday };
        var added = new List<string>();
        var skipped = new List<string>();

        foreach (var day in workDays)
        {
            if (doctor.Schedules.Any(s => s.DayOfWeek == day))
            {
                skipped.Add(day.ToString());
                continue;
            }

            var schedule = DoctorSchedule.Create(doctorId, day, TimeSpan.FromHours(8), TimeSpan.FromHours(17));
            doctor.AddSchedule(schedule);
            await _doctorRepository.AddScheduleAsync(schedule);
            added.Add(day.ToString());
        }

        return Ok(new
        {
            message = "Horarios procesados.",
            doctorId,
            added,
            skipped
        });
    }
}
