using MediatR;

namespace Piedrazul.Application.Modules.Users.Commands.Login;

// Lo que React envía
public record LoginCommand(string Username, string Password)
    : IRequest<LoginResult>;

// Lo que el Handler devuelve
public record LoginResult(
    string Token,
    string Role,
    string FullName,
    DateTime ExpiresAt);