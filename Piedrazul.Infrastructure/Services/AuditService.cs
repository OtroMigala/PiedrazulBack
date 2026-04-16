using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Entities;
using Piedrazul.Infrastructure.Persistence;

namespace Piedrazul.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly PiedrazulDbContext _context;

    public AuditService(PiedrazulDbContext context)
    {
        _context = context;
    }

    public async Task LogAppointmentCreatedAsync(
        Guid performedByUserId,
        Guid appointmentId,
        Guid patientId,
        Guid doctorId,
        DateTime date,
        TimeSpan time)
    {
        var log = AuditLog.Create(
            action: "AppointmentCreated",
            performedByUserId: performedByUserId,
            appointmentId: appointmentId,
            patientId: patientId,
            doctorId: doctorId,
            appointmentDate: date,
            appointmentTime: time);

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogAppointmentRescheduledAsync(
        Guid performedByUserId,
        Guid oldAppointmentId,
        Guid newAppointmentId,
        Guid patientId,
        Guid doctorId,
        DateTime newDate,
        TimeSpan newTime)
    {
        // Registra el re-agendamiento apuntando a la cita original.
        // La nueva cita se puede encontrar via Appointment.RescheduledFromId == oldAppointmentId.
        var log = AuditLog.Create(
            action: "AppointmentRescheduled",
            performedByUserId: performedByUserId,
            appointmentId: oldAppointmentId,
            patientId: patientId,
            doctorId: doctorId,
            appointmentDate: newDate,
            appointmentTime: newTime);

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
