using Microsoft.EntityFrameworkCore;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Infrastructure.Persistence;

namespace Piedrazul.Infrastructure.Repositories;

/// <summary>
/// Repositorio de usuarios del sistema (Admin, Scheduler).
/// Implementa <see cref="IUserRepository"/> usando EF Core
/// sobre la tabla <c>Users</c>.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly PiedrazulDbContext _context;

    public UserRepository(PiedrazulDbContext context)
    {
        _context = context;
    }

    // ── Consultas ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Busca un usuario por nombre de usuario (case-insensitive).
    /// </summary>
    /// <param name="username">Nombre de usuario a buscar.</param>
    /// <returns>El usuario encontrado, o <c>null</c> si no existe.</returns>
    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username.ToLower());

    /// <summary>
    /// Busca un usuario por su ID único.
    /// </summary>
    /// <param name="id">ID UUID del usuario.</param>
    /// <returns>El usuario encontrado, o <c>null</c> si no existe.</returns>
    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    /// <summary>
    /// Verifica si ya existe un usuario registrado con el nombre de usuario dado (case-insensitive).
    /// Usado para validar unicidad antes de crear un nuevo usuario.
    /// </summary>
    /// <param name="username">Nombre de usuario a verificar.</param>
    /// <returns><c>true</c> si el username ya está en uso; <c>false</c> en caso contrario.</returns>
    public async Task<bool> UsernameExistsAsync(string username)
        => await _context.Users.AnyAsync(u => u.Username == username.ToLower());

    // ── Comandos ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Persiste un nuevo usuario en la base de datos.
    /// </summary>
    /// <param name="user">Entidad <see cref="User"/> a insertar.</param>
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente en la base de datos.
    /// </summary>
    /// <param name="user">Entidad <see cref="User"/> con los datos modificados.</param>
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
