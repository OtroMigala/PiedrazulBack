using Piedrazul.Application.Common.Interfaces;

namespace Piedrazul.Infrastructure.Services;

/// <summary>
/// Servicio de hashing de contraseñas.
/// Implementa <see cref="IPasswordHasher"/> usando BCrypt con salt aleatorio embebido.
/// El factor de costo por defecto de BCrypt.Net es 11, adecuado para producción.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Genera un hash BCrypt para la contraseña en texto plano.
    /// El salt aleatorio queda embebido en el hash resultante.
    /// </summary>
    /// <param name="password">Contraseña en texto plano a hashear.</param>
    /// <returns>Hash BCrypt listo para almacenar en base de datos.</returns>
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    /// <summary>
    /// Verifica que una contraseña en texto plano coincide con un hash BCrypt almacenado.
    /// </summary>
    /// <param name="password">Contraseña en texto plano ingresada por el usuario.</param>
    /// <param name="hash">Hash BCrypt almacenado en base de datos.</param>
    /// <returns><c>true</c> si la contraseña es válida; <c>false</c> en caso contrario.</returns>
    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
