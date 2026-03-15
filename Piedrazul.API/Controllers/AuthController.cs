using MediatR;
using Microsoft.AspNetCore.Mvc;
using Piedrazul.Application.Modules.Users.Commands.Login;
using Piedrazul.Application.Modules.Users.Commands.Register;

namespace Piedrazul.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>POST /api/auth/login</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/auth/register
    /// - Sin token: registra un paciente (Role omitido o Patient).
    /// - Con token Admin: puede registrar cualquier rol (Admin, Scheduler, Doctor, Patient).
    /// La validación de autorización para roles elevados se aplica en el handler.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}