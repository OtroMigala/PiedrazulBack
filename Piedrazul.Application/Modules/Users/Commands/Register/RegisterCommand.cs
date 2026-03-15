using MediatR;
using Piedrazul.Domain.Enums;

namespace Piedrazul.Application.Modules.Users.Commands.Register;

public record RegisterCommand(
    string Username,
    string Password,
    string FullName,
    string DocumentId,
    string Phone,
    Gender Gender,
    string? Email,
    DateTime? BirthDate,
    UserRole Role = UserRole.Patient)
    : IRequest<RegisterResult>;

public record RegisterResult(
    string Token,
    string Role,
    string FullName,
    DateTime ExpiresAt);
