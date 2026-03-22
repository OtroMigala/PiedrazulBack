namespace Piedrazul.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public string Action { get; private set; } = null!;
    public Guid PerformedByUserId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public Guid? PatientId { get; private set; }
    public Guid? DoctorId { get; private set; }
    public DateTime? AppointmentDate { get; private set; }
    public TimeSpan? AppointmentTime { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        string action,
        Guid performedByUserId,
        Guid? appointmentId = null,
        Guid? patientId = null,
        Guid? doctorId = null,
        DateTime? appointmentDate = null,
        TimeSpan? appointmentTime = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("La acción de auditoría es obligatoria.");

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action,
            PerformedByUserId = performedByUserId,
            AppointmentId = appointmentId,
            PatientId = patientId,
            DoctorId = doctorId,
            AppointmentDate = appointmentDate?.Date,
            AppointmentTime = appointmentTime,
            OccurredAt = DateTime.UtcNow
        };
    }
}
