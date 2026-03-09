using Microsoft.EntityFrameworkCore;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Infrastructure.Persistence;

namespace Piedrazul.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly PiedrazulDbContext _context;

    public DoctorRepository(PiedrazulDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Doctor>> GetAllActiveAsync()
        => await _context.Doctors
            .Where(d => d.IsActive)
            .OrderBy(d => d.FullName)
            .ToListAsync();

    public async Task<Doctor?> GetByIdAsync(Guid id)
        => await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Doctor?> GetByIdWithSchedulesAsync(Guid id)
        => await _context.Doctors
            .Include(d => d.Schedules)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task AddAsync(Doctor doctor)
    {
        await _context.Doctors.AddAsync(doctor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();
    }

    public async Task AddScheduleAsync(DoctorSchedule schedule)
    {
        await _context.DoctorSchedules.AddAsync(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveScheduleAsync(Guid scheduleId)
    {
        var schedule = await _context.DoctorSchedules.FindAsync(scheduleId);
        if (schedule is not null)
        {
            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }
}