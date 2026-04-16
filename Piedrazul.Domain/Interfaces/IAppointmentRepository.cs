using Piedrazul.Domain.Entities;

namespace Piedrazul.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateTime date);
    Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
    Task<bool> ExistsAsync(Guid doctorId, DateTime date, TimeSpan time);
    Task<bool> PatientHasAppointmentAsync(Guid patientId, Guid doctorId, DateTime date);
    Task<Appointment?> GetByIdAsync(Guid id);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task<IEnumerable<Appointment>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Appointment>> GetHistoryChainAsync(Guid appointmentId);
}