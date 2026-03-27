using Microsoft.EntityFrameworkCore;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Infrastructure.Persistence;

namespace Piedrazul.Infrastructure.Repositories;

public class SchedulingSettingsRepository : ISchedulingSettingsRepository
{
    private readonly PiedrazulDbContext _context;

    public SchedulingSettingsRepository(PiedrazulDbContext context)
    {
        _context = context;
    }

    public async Task<SchedulingSettings?> GetAsync()
    {
        return await _context.SchedulingSettings.FirstOrDefaultAsync();
    }

    public async Task SaveAsync(SchedulingSettings settings)
    {
        var existing = await _context.SchedulingSettings.FirstOrDefaultAsync();

        if (existing is null)
            await _context.SchedulingSettings.AddAsync(settings);
        else
            _context.SchedulingSettings.Update(settings);

        await _context.SaveChangesAsync();
    }
}