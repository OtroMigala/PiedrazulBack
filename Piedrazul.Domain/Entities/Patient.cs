using Piedrazul.Domain.Enums;

namespace Piedrazul.Domain.Entities;

public class Patient
{
    public Guid Id { get; private set; }
    public string DocumentId { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public Gender Gender { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? UserId { get; private set; }

    // EF Core necesita un constructor privado sin parámetros
    private Patient() { }

    public static Patient Create(
        string documentId,
        string fullName,
        string phone,
        Gender gender,
        DateTime? birthDate = null,
        string? email = null)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("El número de documento es obligatorio.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("El nombre completo es obligatorio.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("El celular es obligatorio.");

        return new Patient
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId.Trim(),
            FullName = fullName.Trim(),
            Phone = phone.Trim(),
            Gender = gender,
            BirthDate = birthDate,
            Email = email?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string fullName, string phone, string? email)
    {
        FullName = fullName.Trim();
        Phone = phone.Trim();
        Email = email?.Trim();
    }

    public void AssignUser(Guid userId) => UserId = userId;
}