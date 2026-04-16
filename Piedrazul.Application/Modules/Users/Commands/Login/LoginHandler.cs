using MediatR;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Users.Commands.Login;

/// <summary>
/// Handler para autenticación de usuarios del sistema.
/// Valida credenciales, verifica el hash de contraseña y emite un JWT.
/// Lanza <see cref="UnauthorizedAccessException"/> en cualquier fallo de autenticación;
/// el mensaje es genérico deliberadamente para no revelar si el usuario existe.
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService     _jwtService;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService     jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService     = jwtService;
    }

    /// <summary>
    /// Ejecuta el flujo de autenticación en 3 pasos.
    /// En caso de fallo en cualquier paso, lanza <see cref="UnauthorizedAccessException"/>
    /// con el mismo mensaje genérico para evitar enumeración de usuarios.
    /// </summary>
    public async Task<LoginResult> Handle(
        LoginCommand      request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Buscar usuario activo ─────────────────────────────────────
        // La búsqueda es case-insensitive (username en minúsculas).
        // Usuario inactivo se trata igual que inexistente.
        var user = await _userRepository.GetByUsernameAsync(request.Username.ToLower());
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // ── Paso 2: Verificar contraseña ──────────────────────────────────────
        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // ── Paso 3: Emitir token JWT ──────────────────────────────────────────
        // La expiración (24 h) debe coincidir con la configurada en JwtSettings.
        var token = _jwtService.GenerateToken(user);

        return new LoginResult(
            Token:     token,
            Role:      user.Role.ToString(),
            FullName:  user.FullName,
            ExpiresAt: DateTime.UtcNow.AddHours(24));
    }
}
