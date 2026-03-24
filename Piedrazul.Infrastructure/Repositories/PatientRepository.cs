using Microsoft.EntityFrameworkCore;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Infrastructure.Persistence;

namespace Piedrazul.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly PiedrazulDbContext _context;

    public PatientRepository(PiedrazulDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByDocumentIdAsync(string documentId)
        => await _context.Patients.FirstOrDefaultAsync(p => p.DocumentId == documentId);

    public async Task<bool> DocumentIdExistsAsync(string documentId)
        => await _context.Patients.AnyAsync(p => p.DocumentId == documentId);

    public async Task<Patient?> GetByIdAsync(Guid id)
        => await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Patient?> GetByUserIdAsync(Guid userId)
        => await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task AddAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
    }
}