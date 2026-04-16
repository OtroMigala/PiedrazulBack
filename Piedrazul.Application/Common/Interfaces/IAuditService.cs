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

    Task LogAppointmentRescheduledAsync(
        Guid performedByUserId,
        Guid oldAppointmentId,
        Guid newAppointmentId,
        Guid patientId,
        Guid doctorId,
        DateTime newDate,
        TimeSpan newTime);
}
