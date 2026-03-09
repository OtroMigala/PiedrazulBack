using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Scheduling.Queries.GetAvailableSlots;

namespace Piedrazul.API.Controllers;

[ApiController]
[Route("api/scheduling")]
[Authorize]
public class SchedulingController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchedulingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// HU3 — GET /api/scheduling/available-slots?doctorId=&date=
    /// Usado por el wizard de agendamiento del paciente
    /// </summary>
    [HttpGet("slots")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid doctorId,
        [FromQuery] DateTime date)
    {
        var result = await _mediator.Send(
            new GetAvailableSlotsQuery(doctorId, date));
        return Ok(result);
    }
}