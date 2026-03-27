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

/// <summary>
/// Controlador de médicos y sus horarios de atención.
/// Gestiona el ciclo de vida completo: creación, actualización,
/// desactivación y asignación/remoción de horarios.
/// El listado es público; las operaciones de escritura requieren rol <c>Admin</c>.
/// </summary>
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

    // ── Consultas ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lista todos los médicos activos, con filtro opcional por especialidad.
    /// </summary>
    /// <remarks>
    /// Endpoint público (no requiere autenticación). Usado principalmente
    /// para poblar el selector de médicos en el buscador de citas (HU1).
    ///
    /// Valores válidos para <paramref name="specialty"/>:
    /// <c>NeuralTherapy</c> | <c>Chiropractic</c> | <c>Physiotherapy</c>.
    /// Si se omite, retorna todos los médicos con <c>IsActive = true</c>.
    /// </remarks>
    /// <param name="specialty">Especialidad por la que filtrar (opcional).</param>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Specialty? specialty = null)
    {
        var result = await _mediator.Send(new GetAllDoctorsQuery(specialty));
        return Ok(result);
    }

    // ── Comandos — Médicos ────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuevo médico en el sistema.
    /// </summary>
    /// <remarks>
    /// - <b>201 Created</b>: Médico creado; retorna <c>doctorId</c> y datos básicos.
    /// - <b>400 Bad Request</b>: Payload inválido o campos requeridos ausentes.
    ///
    /// Roles permitidos: <c>Admin</c>.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.DoctorId }, result);
    }

    /// <summary>
    /// Actualiza los datos de un médico existente.
    /// </summary>
    /// <remarks>
    /// - <b>204 No Content</b>: Actualización exitosa.
    /// - <b>404 Not Found</b>: No existe médico con el <paramref name="id"/> proporcionado.
    ///
    /// Roles permitidos: <c>Admin</c>.
    /// </remarks>
    /// <param name="id">ID UUID del médico a actualizar.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorBody body)
    {
        await _mediator.Send(new UpdateDoctorCommand(id, body.FullName, body.Specialty, body.AppointmentIntervalMinutes));
        return NoContent();
    }

    /// <summary>
    /// Desactiva (soft-delete) un médico del sistema.
    /// </summary>
    /// <remarks>
    /// No elimina el registro físicamente; establece <c>IsActive = false</c>.
    /// El médico deja de aparecer en el listado público y en el buscador de citas.
    ///
    /// - <b>204 No Content</b>: Desactivación exitosa.
    /// - <b>404 Not Found</b>: No existe médico con el <paramref name="id"/> proporcionado.
    ///
    /// Roles permitidos: <c>Admin</c>.
    /// </remarks>
    /// <param name="id">ID UUID del médico a desactivar.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDoctorCommand(id));
        return NoContent();
    }

    // ── Comandos — Horarios ───────────────────────────────────────────────────

    /// <summary>
    /// Agrega un horario de atención a un médico existente.
    /// </summary>
    /// <remarks>
    /// Cada horario define un día de la semana y un rango de horas de atención.
    /// Un médico puede tener como máximo un horario por día.
    ///
    /// - <b>200 OK</b>: Horario creado; retorna datos del horario generado.
    /// - <b>404 Not Found</b>: No existe médico con el <paramref name="doctorId"/> proporcionado.
    /// - <b>400 Bad Request</b>: El médico ya tiene horario para ese día, o el rango es inválido.
    ///
    /// Roles permitidos: <c>Admin</c>.
    /// </remarks>
    /// <param name="doctorId">ID UUID del médico al que se le asigna el horario.</param>
    [HttpPost("{doctorId:guid}/schedules")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddSchedule(Guid doctorId, [FromBody] AddScheduleBody body)
    {
        var result = await _mediator.Send(new AddScheduleCommand(doctorId, body.DayOfWeek, body.StartTime, body.EndTime));
        return Ok(result);
    }

    /// <summary>
    /// Elimina un horario de atención de un médico.
    /// </summary>
    /// <remarks>
    /// - <b>204 No Content</b>: Horario eliminado correctamente.
    /// - <b>404 Not Found</b>: No existe el médico o el horario con los IDs proporcionados.
    ///
    /// Roles permitidos: <c>Admin</c>.
    /// </remarks>
    /// <param name="doctorId">ID UUID del médico dueño del horario.</param>
    /// <param name="scheduleId">ID UUID del horario a eliminar.</param>
    [HttpDelete("{doctorId:guid}/schedules/{scheduleId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveSchedule(Guid doctorId, Guid scheduleId)
    {
        await _mediator.Send(new RemoveScheduleCommand(doctorId, scheduleId));
        return NoContent();
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────

/// <summary>
/// Payload para actualizar los datos editables de un médico.
/// </summary>
/// <param name="FullName">Nombre completo con título (ej: "Dr. García").</param>
/// <param name="Specialty">Especialidad del médico: <c>NeuralTherapy</c> | <c>Chiropractic</c> | <c>Physiotherapy</c>.</param>
/// <param name="AppointmentIntervalMinutes">Duración en minutos de cada franja de atención (ej: 30).</param>
public record UpdateDoctorBody(
    string FullName,
    Specialty Specialty,
    int AppointmentIntervalMinutes);

/// <summary>
/// Payload para agregar un horario de atención a un médico.
/// </summary>
/// <param name="DayOfWeek">Día de la semana (ej: <c>Monday</c>).</param>
/// <param name="StartTime">Hora de inicio de atención (ej: <c>08:00:00</c>).</param>
/// <param name="EndTime">Hora de fin de atención (ej: <c>17:00:00</c>).</param>
public record AddScheduleBody(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime);
