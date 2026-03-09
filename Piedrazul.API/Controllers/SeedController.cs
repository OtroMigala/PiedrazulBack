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

    /// <summary>POST /api/seed/doctor — Crea un doctor de prueba y retorna su Id</summary>
    [HttpPost("doctor")]
    public async Task<IActionResult> CreateDoctor()
    {
        var doctor = Doctor.Create(
            "Dr. Carlos Pérez",
            DoctorType.Doctor,
            Specialty.NeuralTherapy,
            30);

        await _doctorRepository.AddAsync(doctor);

        return Ok(new
        {
            message = "Doctor creado.",
            doctorId = doctor.Id,
            fullName = doctor.FullName,
            specialty = doctor.Specialty.ToString(),
            intervalMinutes = doctor.AppointmentIntervalMinutes
        });
    }
}
