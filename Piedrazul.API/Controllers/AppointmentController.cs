using System.Globalization;
using System.Security.Claims;
using CsvHelper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Appointments.Commands.CreateAppointment;
using Piedrazul.Application.Modules.Appointments.Commands.RescheduleAppointment;
using Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentHistory;
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
    /// Agenda diaria: todas las citas de un día (sin filtro por médico).
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
    /// Citas de un médico en una fecha.
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
    /// Agendador crea cita manualmente.
    /// Roles: Admin, Scheduler
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Scheduler")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
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

    /// <summary>
    /// HU-R — PUT /api/appointments/{id}/reschedule
    /// Re-agenda una cita existente.
    /// Roles: Admin, Scheduler, Doctor (solo sus propias citas)
    /// </summary>
    [HttpPut("{id:guid}/reschedule")]
    [Authorize(Roles = "Admin,Scheduler,Doctor")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var command = new RescheduleAppointmentCommand(
            AppointmentId: id,
            NewDate: request.NewDate,
            NewTime: request.NewTime,
            RequestingUserId: userId,
            RequestingUserRole: role);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// HU-H — GET /api/appointments/{id}/history
    /// Historial completo de re-agendamientos de una cita (cadena de versiones).
    /// Roles: Admin, Scheduler, Doctor
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [Authorize(Roles = "Admin,Scheduler,Doctor")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var result = await _mediator.Send(new GetAppointmentHistoryQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// HU-E — GET /api/appointments/export?doctorId=&date=
    /// Exporta las citas de un médico en una fecha como archivo CSV.
    /// Roles: Admin, Scheduler
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Admin,Scheduler")]
    public async Task<IActionResult> ExportCsv([FromQuery] Guid doctorId, [FromQuery] DateTime date)
    {
        var result = await _mediator.Send(
            new GetAppointmentsByDoctorAndDateQuery(doctorId, date));

        var records = result.Appointments.Select(a => new AppointmentCsvRecord(
            Fecha: date.ToString("dd/MM/yyyy"),
            Hora: a.Time,
            Paciente: a.PatientName,
            Documento: a.DocumentId,
            Especialidad: a.Specialty,
            Estado: a.Status));

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, leaveOpen: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
        writer.Flush();

        return File(ms.ToArray(), "text/csv; charset=utf-8", $"citas_{date:yyyy-MM-dd}.csv");
    }
}

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

public record RescheduleAppointmentRequest(
    DateTime NewDate,
    TimeSpan NewTime);

public record AppointmentCsvRecord(
    string Fecha,
    string Hora,
    string Paciente,
    string Documento,
    string Especialidad,
    string Estado);
