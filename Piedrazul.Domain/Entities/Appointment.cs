using Piedrazul.Domain.Enums;
using Piedrazul.Domain.Exceptions;

namespace Piedrazul.Domain.Entities;

public class Appointment
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTime Date { get; private set; }
    public TimeSpan Time { get; private set; }
    public Specialty Specialty { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Para re-agendamiento
    public Guid? RescheduledFromId { get; private set; }
    public DateTime? RescheduledAt { get; private set; }
    public Guid? RescheduledByUserId { get; private set; }

    // Navegación
    public Patient? Patient { get; private set; }
    public Doctor? Doctor { get; private set; }

    private Appointment() { }

    public static Appointment Create(
        Guid patientId,
        Guid doctorId,
        DateTime date,
        TimeSpan time,
        Specialty specialty,
        Guid createdByUserId)
    {
        if (date.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("No se puede agendar una cita en una fecha pasada.");

        return new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            Date = date.Date,
            Time = time,
            Specialty = specialty,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public Appointment Reschedule(DateTime newDate, TimeSpan newTime, Guid rescheduledByUserId)
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("No se puede re-agendar una cita cancelada.");

        // Marcar la cita actual como re-agendada
        Status = AppointmentStatus.Rescheduled;
        RescheduledAt = DateTime.UtcNow;
        RescheduledByUserId = rescheduledByUserId;

        // Crear la nueva cita vinculada
        return new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = PatientId,
            DoctorId = DoctorId,
            Date = newDate.Date,
            Time = newTime,
            Specialty = Specialty,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = rescheduledByUserId,
            RescheduledFromId = Id
        };
    }

    public void Cancel() => Status = AppointmentStatus.Cancelled;
    public void Complete() => Status = AppointmentStatus.Completed;
}