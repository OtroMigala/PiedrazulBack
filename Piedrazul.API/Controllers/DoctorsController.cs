using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Doctors.Commands.AddSchedule;
using Piedrazul.Application.Modules.Doctors.Commands.CreateDoctor;
using Piedrazul.Application.Modules.Doctors.Commands.DeleteDoctor;
using Piedrazul.Application.Modules.Doctors.Commands.RemoveSchedule;
using Piedrazul.Application.Modules.Doctors.Commands.UpdateDoctor;
using Piedrazul.Application.Modules.Doctors.Queries.GetAllDoctors;
using Piedrazul.Domain.Enums;

namespace Piedrazul.API.Controllers;

[ApiController]
[Route("api/doctors")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// GET /api/doctors?specialty=NeuralTherapy — Lista médicos activos.
    /// El parámetro specialty es opcional; si se omite devuelve todos.
    /// Valores válidos: NeuralTherapy | Chiropractic | Physiotherapy
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] Specialty? specialty = null)
    {
        var result = await _mediator.Send(new GetAllDoctorsQuery(specialty));
        return Ok(result);
    }

    /// <summary>POST /api/doctors — Crea un médico (Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDoctorCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.DoctorId }, result);
    }

    /// <summary>PUT /api/doctors/{id} — Actualiza un médico (Admin)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorBody body)
    {
        await _mediator.Send(new UpdateDoctorCommand(id, body.FullName, body.Specialty, body.AppointmentIntervalMinutes));
        return NoContent();
    }

    /// <summary>DELETE /api/doctors/{id} — Desactiva un médico (Admin)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDoctorCommand(id));
        return NoContent();
    }

    /// <summary>POST /api/doctors/{doctorId}/schedules — Agrega horario al médico (Admin)</summary>
    [HttpPost("{doctorId:guid}/schedules")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddSchedule(Guid doctorId, [FromBody] AddScheduleBody body)
    {
        var result = await _mediator.Send(new AddScheduleCommand(doctorId, body.DayOfWeek, body.StartTime, body.EndTime));
        return Ok(result);
    }

    /// <summary>DELETE /api/doctors/{doctorId}/schedules/{scheduleId} — Elimina horario (Admin)</summary>
    [HttpDelete("{doctorId:guid}/schedules/{scheduleId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveSchedule(Guid doctorId, Guid scheduleId)
    {
        await _mediator.Send(new RemoveScheduleCommand(doctorId, scheduleId));
        return NoContent();
    }
}

public record UpdateDoctorBody(
    string FullName,
    Piedrazul.Domain.Enums.Specialty Specialty,
    int AppointmentIntervalMinutes);

public record AddScheduleBody(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime);