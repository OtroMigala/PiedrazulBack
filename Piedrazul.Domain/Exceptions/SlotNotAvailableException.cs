namespace Piedrazul.Domain.Exceptions;

public class SlotNotAvailableException : Exception
{
    public SlotNotAvailableException(DateTime date, TimeSpan time)
        : base($"El horario {time:hh\\:mm} del {date:dd/MM/yyyy} no está disponible.") { }
}