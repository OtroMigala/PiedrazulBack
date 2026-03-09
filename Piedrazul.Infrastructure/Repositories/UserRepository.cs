using Microsoft.EntityFrameworkCore;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Infrastructure.Persistence;

namespace Piedrazul.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PiedrazulDbContext _context;

    public UserRepository(PiedrazulDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username.ToLower());

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<bool> UsernameExistsAsync(string username)
        => await _context.Users.AnyAsync(u => u.Username == username.ToLower());

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}