using Piedrazul.Application.Common.Interfaces;
using Piedrazul.Domain.Entities;
using Piedrazul.Infrastructure.Persistence;

namespace Piedrazul.Infrastructure.Services;

/// <summary>
/// Servicio de auditoría. Registra acciones relevantes del sistema
/// en la tabla <c>AuditLogs</c> para trazabilidad y cumplimiento.
/// </summary>
public class AuditService : IAuditService
{
    private readonly PiedrazulDbContext _context;

    public AuditService(PiedrazulDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registra en auditoría la creación exitosa de una cita.
    /// </summary>
    /// <param name="performedByUserId">ID del usuario (Admin o Scheduler) que realizó la acción.</param>
    /// <param name="appointmentId">ID de la cita recién creada.</param>
    /// <param name="patientId">ID del paciente asociado a la cita.</param>
    /// <param name="doctorId">ID del médico asignado a la cita.</param>
    /// <param name="date">Fecha de la cita (componente de fecha).</param>
    /// <param name="time">Hora de la cita (componente de tiempo).</param>
    public async Task LogAppointmentCreatedAsync(
        Guid     performedByUserId,
        Guid     appointmentId,
        Guid     patientId,
        Guid     doctorId,
        DateTime date,
        TimeSpan time)
    {
        var log = AuditLog.Create(
            action:          "AppointmentCreated",
            performedByUserId: performedByUserId,
            appointmentId:   appointmentId,
            patientId:       patientId,
            doctorId:        doctorId,
            appointmentDate: date,
            appointmentTime: time);

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
