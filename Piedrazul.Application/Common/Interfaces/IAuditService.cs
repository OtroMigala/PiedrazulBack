namespace Piedrazul.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAppointmentCreatedAsync(
        Guid performedByUserId,
        Guid appointmentId,
        Guid patientId,
        Guid doctorId,
        DateTime date,
        TimeSpan time);
}
