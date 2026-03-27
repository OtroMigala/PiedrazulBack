using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Appointments.Commands.BookAppointment;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.API.Controllers;

[ApiController]
[Route("api/patient/appointments")]
[Authorize(Roles = "Patient")]
public class PatientAppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppointmentRepository _appointmentRepository;

    public PatientAppointmentsController(IMediator mediator, IAppointmentRepository appointmentRepository)
    {
        _mediator = mediator;
        _appointmentRepository = appointmentRepository;
    }

    /// <summary>POST /api/patient/appointments — Paciente agenda su propia cita</summary>
    [HttpPost]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentBody body)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (!TimeSpan.TryParse(body.Time, out var time))
            return BadRequest(new { error = "Formato de hora inválido. Use HH:mm:ss o HH:mm." });

        var result = await _mediator.Send(new BookAppointmentCommand(
            userId,
            body.DoctorId,
            body.Date,
            time,
            body.CaptchaToken));

        return Ok(result);
    }

    /// <summary>GET /api/patient/appointments — Paciente consulta sus propias citas</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var appointments = await _appointmentRepository.GetByUserIdAsync(userId);
        return Ok(appointments.Select(a => new
        {
            id = a.Id,
            date = a.Date.ToString("dd/MM/yyyy"),
            time = a.Time.ToString(@"hh\:mm"),
            doctorName = a.Doctor!.FullName,
            specialty = a.Specialty.ToString(),
            status = a.Status.ToString(),
            phone = a.Patient!.Phone,       
            documentId = a.Patient!.DocumentId, 
            patientName = a.Patient!.FullName, 
        }));
    }
}

public record BookAppointmentBody(Guid DoctorId, DateTime Date, string Time, string CaptchaToken);