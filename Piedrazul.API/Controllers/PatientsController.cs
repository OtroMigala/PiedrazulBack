using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Patients.Queries.GetPatientByDocument;

namespace Piedrazul.API.Controllers;

/// <summary>
/// Controlador de pacientes.
/// Expone operaciones de consulta y registro sobre la entidad Patient.
/// Todos los endpoints requieren autenticación JWT.
/// </summary>
[ApiController]
[Route("api/patients")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ── Consultas ─────────────────────────────────────────────────────────────

    /// <summary>
    /// CA2.1 — Busca un paciente existente por número de documento.
    /// </summary>
    /// <remarks>
    /// Usado en el flujo de agendamiento para determinar si el paciente ya está
    /// registrado en el sistema antes de crear una cita.
    ///
    /// - <b>200 OK</b>: Paciente encontrado; retorna sus datos completos.
    /// - <b>404 Not Found</b>: No existe paciente con ese documento;
    ///   el frontend debe habilitar el formulario de registro nuevo.
    ///
    /// Roles permitidos: <c>Admin</c>, <c>Scheduler</c>.
    /// </remarks>
    /// <param name="documentId">Número de documento del paciente (cédula, pasaporte, etc.).</param>
    [HttpGet("by-document")]
    [Authorize(Roles = "Admin,Scheduler")]
    [ProducesResponseType(typeof(GetPatientByDocumentQuery), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByDocument([FromQuery] string documentId)
    {
        var result = await _mediator.Send(new GetPatientByDocumentQuery(documentId));

        if (result is null)
            return NotFound(new { message = "Paciente no encontrado. Puede registrarlo como nuevo." });

        return Ok(result);
    }
}
