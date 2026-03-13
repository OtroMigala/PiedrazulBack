using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Patients.Queries.GetPatientByDocument;

namespace Piedrazul.API.Controllers;

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

    /// <summary>
    /// CA2.1 — GET /api/patients/by-document?documentId=
    /// Busca un paciente existente por número de documento.
    /// 200 OK con datos si existe; 404 si no existe (el frontend habilita registro nuevo).
    /// Roles: Admin, Scheduler
    /// </summary>
    [HttpGet("by-document")]
    [Authorize(Roles = "Admin,Scheduler")]
    public async Task<IActionResult> GetByDocument([FromQuery] string documentId)
    {
        var result = await _mediator.Send(new GetPatientByDocumentQuery(documentId));

        if (result is null)
            return NotFound(new { message = "Paciente no encontrado. Puede registrarlo como nuevo." });

        return Ok(result);
    }
}
