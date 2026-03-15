using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Appointments.Commands.BookAppointment;

namespace Piedrazul.API.Controllers;

[ApiController]
[Route("api/patient/appointments")]
[Authorize(Roles = "Patient")]
public class PatientAppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientAppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
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
}

/// <param name="CaptchaToken">
/// Token generado por el widget anti-bot en el frontend.
/// Mock funcional en esta iteración; integrar con reCAPTCHA/hCaptcha en producción.
/// </param>
public record BookAppointmentBody(Guid DoctorId, DateTime Date, string Time, string CaptchaToken);
