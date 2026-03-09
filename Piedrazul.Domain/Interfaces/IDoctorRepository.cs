using Piedrazul.Domain.Entities;

namespace Piedrazul.Domain.Interfaces;

public interface IDoctorRepository
{
    Task<IEnumerable<Doctor>> GetAllActiveAsync();
    Task<Doctor?> GetByIdAsync(Guid id);
    Task<Doctor?> GetByIdWithSchedulesAsync(Guid id);
    Task AddAsync(Doctor doctor);
    Task UpdateAsync(Doctor doctor);
    Task AddScheduleAsync(DoctorSchedule schedule);
    Task RemoveScheduleAsync(Guid scheduleId);
}