using MediatR;
using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Interfaces;

namespace Piedrazul.Application.Modules.Users.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Buscar el usuario
        var user = await _userRepository.GetByUsernameAsync(request.Username.ToLower());

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // 2. Verificar contraseña
        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        // 3. Generar token JWT
        var token = _jwtService.GenerateToken(user);

        return new LoginResult(
            Token: token,
            Role: user.Role.ToString(),
            FullName: user.FullName,
            ExpiresAt: DateTime.UtcNow.AddHours(24));
    }
}