using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Piedrazul.Application.Common.Interfaces;

namespace Piedrazul.Infrastructure.Services;

/// <summary>
/// Servicio que expone la identidad del usuario autenticado en la solicitud HTTP actual.
/// Implementa <see cref="ICurrentUserService"/> extrayendo claims del JWT
/// a través de <see cref="IHttpContextAccessor"/>.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// ID del usuario autenticado, extraído del claim <c>NameIdentifier</c>.
    /// Retorna <c>null</c> si no hay sesión activa o el claim no es un UUID válido.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    /// <summary>
    /// Rol del usuario autenticado, extraído del claim <c>Role</c>.
    /// Retorna <c>null</c> si no hay sesión activa.
    /// Valores posibles: <c>Admin</c> | <c>Scheduler</c> | <c>Patient</c>.
    /// </summary>
    public string? Role
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

    /// <summary>
    /// Indica si la solicitud HTTP actual tiene una identidad autenticada.
    /// Retorna <c>false</c> si no hay contexto HTTP disponible.
    /// </summary>
    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
