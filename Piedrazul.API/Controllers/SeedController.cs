using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Enums;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.API.Controllers;

/// <summary>
/// Endpoints temporales para datos de prueba.
/// SOLO disponibles en entorno Development.
/// ELIMINAR en producción.
/// </summary>
[ApiController]
[Route("api/seed")]
[ApiExplorerSettings(IgnoreApi = true)] // Ocultar de Swagger en entornos compartidos
public class SeedController : ControllerBase
{
    // ── Credenciales de semilla ────────────────────────────────────────────────
    private const string AdminUsername     = "admin";
    private const string AdminPassword     = "admin123";
    private const string SchedulerUsername = "scheduler";
    private const string SchedulerPassword = "scheduler123";

    // ── Días laborales reutilizables ───────────────────────────────────────────
    private static readonly DayOfWeek[] WorkDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    ];

    // ── Dependencias ───────────────────────────────────────────────────────────
    private readonly IUserRepository   _userRepository;
    private readonly IPasswordHasher   _passwordHasher;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IWebHostEnvironment _env;

    public SeedController(
        IUserRepository      userRepository,
        IPasswordHasher      passwordHasher,
        IDoctorRepository    doctorRepository,
        IWebHostEnvironment  env)
    {
        _userRepository   = userRepository;
        _passwordHasher   = passwordHasher;
        _doctorRepository = doctorRepository;
        _env              = env;
    }

    // ── Guard de entorno ───────────────────────────────────────────────────────
    /// <summary>
    /// Retorna 403 si el entorno no es Development.
    /// Llamar al inicio de cada endpoint.
    /// </summary>
    private IActionResult? DenyIfNotDevelopment()
    {
        if (!_env.IsDevelopment())
            return StatusCode(403, new { message = "Este endpoint solo está disponible en entorno Development." });
        return null;
    }

    // ── Endpoints ──────────────────────────────────────────────────────────────

    /// <summary>POST /api/seed/admin — Crea el usuario admin si no existe.</summary>
    [HttpPost("admin")]
    public async Task<IActionResult> CreateAdmin()
    {
        if (DenyIfNotDevelopment() is { } guard) return guard;

        if (await _userRepository.UsernameExistsAsync(AdminUsername))
            return Conflict(new { message = $"El usuario '{AdminUsername}' ya existe." });

        var user = Domain.Entities.User.Create(
            AdminUsername,
            _passwordHasher.Hash(AdminPassword),
            "Administrador",
            UserRole.Admin,
            "admin@piedrazul.com");

        await _userRepository.AddAsync(user);

        return Ok(new
        {
            message  = "Usuario admin creado.",
            username = AdminUsername,
            password = AdminPassword
        });
    }

    /// <summary>POST /api/seed/scheduler — Crea el usuario agendador si no existe.</summary>
    [HttpPost("scheduler")]
    public async Task<IActionResult> CreateScheduler()
    {
        if (DenyIfNotDevelopment() is { } guard) return guard;

        if (await _userRepository.UsernameExistsAsync(SchedulerUsername))
            return Conflict(new { message = $"El usuario '{SchedulerUsername}' ya existe." });

        var user = Domain.Entities.User.Create(
            SchedulerUsername,
            _passwordHasher.Hash(SchedulerPassword),
            "Agendador Principal",
            UserRole.Scheduler,
            "scheduler@piedrazul.com");

        await _userRepository.AddAsync(user);

        return Ok(new
        {
            message  = "Usuario scheduler creado.",
            username = SchedulerUsername,
            password = SchedulerPassword
        });
    }

    /// <summary>
    /// POST /api/seed/doctor — Crea un doctor de prueba con horarios Lun-Vie 08:00-17:00.
    /// Es idempotente por nombre: si ya existe un doctor con el mismo fullName, retorna 409.
    /// </summary>
    [HttpPost("doctor")]
    public async Task<IActionResult> CreateDoctor()
    {
        if (DenyIfNotDevelopment() is { } guard) return guard;

        const string seedDoctorName = "Dr. Carlos Pérez";

        // Guard de idempotencia: evita crear duplicados si se llama más de una vez
        var existing = await _doctorRepository.GetByFullNameAsync(seedDoctorName);
        if (existing is not null)
            return Conflict(new
            {
                message  = $"El doctor '{seedDoctorName}' ya existe.",
                doctorId = existing.Id
            });

        var doctor = Doctor.Create(
            seedDoctorName,
            DoctorType.Doctor,
            Specialty.NeuralTherapy,
            30);

        await _doctorRepository.AddAsync(doctor);

        foreach (var day in WorkDays)
        {
            var schedule = DoctorSchedule.Create(
                doctor.Id,
                day,
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(17));

            doctor.AddSchedule(schedule);
            await _doctorRepository.AddScheduleAsync(schedule);
        }

        return Ok(new
        {
            message         = "Doctor creado con horarios Lun-Vie 08:00-17:00.",
            doctorId        = doctor.Id,
            fullName        = doctor.FullName,
            specialty       = doctor.Specialty.ToString(),
            intervalMinutes = doctor.AppointmentIntervalMinutes,
            scheduledDays   = WorkDays.Select(d => d.ToString()).ToArray()
        });
    }

    /// <summary>
    /// POST /api/seed/schedules/{doctorId} — Asigna horarios Lun-Vie 08:00-17:00 a un doctor existente.
    /// Omite días que ya tengan horario configurado.
    /// </summary>
    [HttpPost("schedules/{doctorId:guid}")]
    public async Task<IActionResult> SeedSchedules(Guid doctorId)
    {
        if (DenyIfNotDevelopment() is { } guard) return guard;

        var doctor = await _doctorRepository.GetByIdWithSchedulesAsync(doctorId);
        if (doctor is null)
            return NotFound(new { message = $"No se encontró médico con id {doctorId}." });

        var added   = new List<string>();
        var skipped = new List<string>();

        foreach (var day in WorkDays)
        {
            if (doctor.Schedules.Any(s => s.DayOfWeek == day))
            {
                skipped.Add(day.ToString());
                continue;
            }

            var schedule = DoctorSchedule.Create(
                doctorId,
                day,
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(17));

            doctor.AddSchedule(schedule);
            await _doctorRepository.AddScheduleAsync(schedule);
            added.Add(day.ToString());
        }

        return Ok(new
        {
            message  = "Horarios procesados.",
            doctorId,
            added,
            skipped
        });
    }
}
