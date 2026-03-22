using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Appointments.Commands.CreateAppointment;
using Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDate;
using Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDoctorAndDate;
using Piedrazul.Domain.Enums;

namespace Piedrazul.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// HU1-A — GET /api/appointments?date=YYYY-MM-DD
    /// Agenda Diaria: todas las citas de un día (sin filtro por médico)
    /// Roles: Admin, Scheduler
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Scheduler")]
    public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
    {
        var result = await _mediator.Send(new GetAppointmentsByDateQuery(date));
        return Ok(result);
    }

    /// <summary>
    /// HU1-B — GET /api/appointments/by-doctor?doctorId=&date=
    /// Roles: Admin, Scheduler
    /// </summary>
    [HttpGet("by-doctor")]
    [Authorize(Roles = "Admin,Scheduler")]
    public async Task<IActionResult> GetByDoctorAndDate(
        [FromQuery] Guid doctorId,
        [FromQuery] DateTime date,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(
            new GetAppointmentsByDoctorAndDateQuery(doctorId, date, search));
        return Ok(result);
    }

    /// <summary>
    /// HU2 — POST /api/appointments
    /// Agendador crea cita manualmente
    /// Roles: Admin, Scheduler
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Scheduler")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        // Extraer el userId del JWT
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new CreateAppointmentCommand(
            DocumentId: request.DocumentId,
            FullName: request.FullName,
            Phone: request.Phone,
            Gender: request.Gender,
            BirthDate: request.BirthDate,
            Email: request.Email,
            DoctorId: request.DoctorId,
            Date: request.Date,
            Time: request.Time,
            CreatedByUserId: userId);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByDate), result);
    }
}

// DTO de entrada para no exponer el Command directamente
public record CreateAppointmentRequest(
    string DocumentId,
    string FullName,
    string Phone,
    Gender Gender,
    DateTime? BirthDate,
    string? Email,
    Guid DoctorId,
    DateTime Date,
    TimeSpan Time);