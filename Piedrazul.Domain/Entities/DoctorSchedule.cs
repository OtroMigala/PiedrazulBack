namespace Piedrazul.Domain.Entities;

public class DoctorSchedule
{
    public Guid Id { get; private set; }
    public Guid DoctorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }

    private DoctorSchedule() { }

    public static DoctorSchedule Create(Guid doctorId, DayOfWeek day, TimeSpan start, TimeSpan end)
    {
        if (end <= start)
            throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio.");

        return new DoctorSchedule
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            DayOfWeek = day,
            StartTime = start,
            EndTime = end
        };
    }
}