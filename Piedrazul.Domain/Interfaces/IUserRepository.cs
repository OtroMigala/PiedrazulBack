using Piedrazul.Domain.Entities;

namespace Piedrazul.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> UsernameExistsAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}