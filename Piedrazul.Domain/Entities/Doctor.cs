using Piedrazul.Domain.Enums;

namespace Piedrazul.Domain.Entities;

public class Doctor
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = null!;
    public DoctorType Type { get; private set; }
    public Specialty Specialty { get; private set; }
    public int AppointmentIntervalMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Relación con el usuario del sistema (opcional)
    public Guid? UserId { get; private set; }

    // Horarios de disponibilidad por día
    private readonly List<DoctorSchedule> _schedules = new();
    public IReadOnlyCollection<DoctorSchedule> Schedules => _schedules.AsReadOnly();

    private Doctor() { }

    public static Doctor Create(
        string fullName,
        DoctorType type,
        Specialty specialty,
        int appointmentIntervalMinutes)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("El nombre del profesional es obligatorio.");
        if (appointmentIntervalMinutes <= 0)
            throw new ArgumentException("El intervalo entre citas debe ser mayor a 0 minutos.");

        return new Doctor
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Type = type,
            Specialty = specialty,
            AppointmentIntervalMinutes = appointmentIntervalMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string fullName, Specialty specialty, int intervalMinutes)
    {
        FullName = fullName.Trim();
        Specialty = specialty;
        AppointmentIntervalMinutes = intervalMinutes;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void AssignUser(Guid userId) => UserId = userId;

    public void AddSchedule(DoctorSchedule schedule)
    {
        if (_schedules.Any(s => s.DayOfWeek == schedule.DayOfWeek))
            throw new InvalidOperationException($"Ya existe un horario configurado para el día {schedule.DayOfWeek}.");
        _schedules.Add(schedule);
    }

    public void RemoveSchedule(Guid scheduleId)
    {
        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule is null)
            throw new InvalidOperationException("El horario no existe.");
        _schedules.Remove(schedule);
    }

    // Calcula los slots disponibles para una fecha dada
    public IEnumerable<TimeSpan> GetAvailableSlots(DateTime date, IEnumerable<TimeSpan> occupiedSlots)
    {
        var dayOfWeek = date.DayOfWeek;
        var schedule = _schedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

        if (schedule is null || !IsActive)
            return Enumerable.Empty<TimeSpan>();

        var slots = new List<TimeSpan>();
        var current = schedule.StartTime;

        while (current + TimeSpan.FromMinutes(AppointmentIntervalMinutes) <= schedule.EndTime)
        {
            if (!occupiedSlots.Contains(current))
                slots.Add(current);
            current = current.Add(TimeSpan.FromMinutes(AppointmentIntervalMinutes));
        }

        return slots;
    }
}