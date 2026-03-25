namespace Piedrazul.Domain.Entities;

public class SchedulingSettings
{
    public Guid Id { get; private set; }
    public int WeeksAhead { get; private set; }

    private SchedulingSettings() { }

    public static SchedulingSettings CreateDefault()
    {
        return new SchedulingSettings
        {
            Id = Guid.NewGuid(),
            WeeksAhead = 4
        };
    }

    public void UpdateWeeksAhead(int weeksAhead)
    {
        if (weeksAhead <= 0)
            throw new ArgumentException("La ventana de agendamiento debe ser mayor a 0 semanas.");

        WeeksAhead = weeksAhead;
    }
}