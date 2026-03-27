using Microsoft.EntityFrameworkCore;
using Piedrazul.Domain.Entities;
using Piedrazul.Domain.Interfaces;
using Piedrazul.Infrastructure.Persistence;

namespace Piedrazul.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly PiedrazulDbContext _context;

    public AppointmentRepository(PiedrazulDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateTime date)
        => await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.DoctorId == doctorId && a.Date == date.Date)
            .OrderBy(a => a.Time)
            .ToListAsync();

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
        => await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.Date == date.Date)
            .OrderBy(a => a.Time)
            .ToListAsync();

    public async Task<bool> ExistsAsync(Guid doctorId, DateTime date, TimeSpan time)
        => await _context.Appointments
            .AnyAsync(a => a.DoctorId == doctorId
                        && a.Date == date.Date
                        && a.Time == time
                        && a.Status != Domain.Enums.AppointmentStatus.Cancelled);

    public async Task<bool> PatientHasAppointmentAsync(Guid patientId, Guid doctorId, DateTime date)
        => await _context.Appointments
            .AnyAsync(a => a.PatientId == patientId
                        && a.DoctorId == doctorId
                        && a.Date == date.Date
                        && a.Status == Domain.Enums.AppointmentStatus.Scheduled);

    public async Task<Appointment?> GetByIdAsync(Guid id)
        => await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId)
    => await _context.Appointments
        .Include(a => a.Doctor)
        .Include(a => a.Patient)
        .Where(a => a.Patient!.UserId == userId)
        .OrderByDescending(a => a.Date)
        .ThenBy(a => a.Time)
        .ToListAsync();
}