using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Piedrazul.Application.Modules.Scheduling.Queries.GetAvailableSlots;

namespace Piedrazul.API.Controllers;

[ApiController]
[Route("api/scheduling")]
[Authorize]
public class SchedulingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public SchedulingController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    /// <summary>
    /// CA3.3 — GET /api/scheduling/config (sin auth)
    /// Devuelve la ventana de agendamiento para que el frontend
    /// deshabilite fechas fuera del rango en el calendario.
    /// </summary>
    [HttpGet("config")]
    [AllowAnonymous]
    public IActionResult GetConfig()
    {
        var weeksAhead = int.TryParse(_configuration["Scheduling:WeeksAhead"], out var w) ? w : 4;
        var today = DateTime.UtcNow.Date;
        var maxDate = today.AddDays(weeksAhead * 7);

        return Ok(new
        {
            weeksAhead,
            minDate = today.ToString("yyyy-MM-dd"),
            maxDate = maxDate.ToString("yyyy-MM-dd")
        });
    }

    /// <summary>
    /// HU3 — GET /api/scheduling/slots?doctorId=&date=
    /// Consulta los slots disponibles. Usado por el paciente en el wizard
    /// de auto-agendamiento y por el Scheduler al crear citas manualmente.
    /// Roles: Admin, Scheduler, Patient
    /// </summary>
    [HttpGet("slots")]
    [Authorize(Roles = "Admin,Scheduler,Patient")]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid doctorId,
        [FromQuery] DateTime date)
    {
        var result = await _mediator.Send(
            new GetAvailableSlotsQuery(doctorId, date));
        return Ok(result);
    }
}