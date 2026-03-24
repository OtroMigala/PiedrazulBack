using Piedrazul.Domain.Entities;

namespace Piedrazul.Domain.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByDocumentIdAsync(string documentId);
    Task<bool> DocumentIdExistsAsync(string documentId);
    Task<Patient?> GetByIdAsync(Guid id);
    Task<Patient?> GetByUserIdAsync(Guid userId);
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
}