using Piedrazul.Domain.Entities;

namespace Piedrazul.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}