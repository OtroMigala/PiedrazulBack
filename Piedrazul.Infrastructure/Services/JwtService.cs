using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Entities;

namespace Piedrazul.Infrastructure.Services;

/// <summary>
/// Servicio de generación de tokens JWT.
/// Implementa <see cref="IJwtService"/> usando HMAC-SHA256 como algoritmo de firma.
/// Lee la configuración desde <c>appsettings.json</c> bajo la sección <c>Jwt</c>
/// (<c>Jwt:Secret</c>, <c>Jwt:Issuer</c>, <c>Jwt:Audience</c>).
/// </summary>
public class JwtService : IJwtService
{
    // Duración del token. Debe coincidir con lo retornado en LoginResult.ExpiresAt.
    private const int TokenExpirationHours = 24;

    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Genera un token JWT firmado para el usuario autenticado.
    /// </summary>
    /// <remarks>
    /// Claims incluidos en el token:
    /// <list type="bullet">
    ///   <item><c>NameIdentifier</c> — ID UUID del usuario.</item>
    ///   <item><c>Name</c> — Nombre de usuario (username).</item>
    ///   <item><c>Role</c> — Rol del usuario (<c>Admin</c> | <c>Scheduler</c> | <c>Patient</c>).</item>
    ///   <item><c>fullName</c> — Nombre completo legible del usuario.</item>
    /// </list>
    /// </remarks>
    /// <param name="user">Entidad <see cref="User"/> autenticada cuyos datos se incluyen como claims.</param>
    /// <returns>Token JWT firmado como string compacto (formato <c>header.payload.signature</c>).</returns>
    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Role,           user.Role.ToString()),
            new Claim("fullName",                user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"],
            audience:           _configuration["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(TokenExpirationHours),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
