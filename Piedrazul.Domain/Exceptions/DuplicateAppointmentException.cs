namespace Piedrazul.Domain.Exceptions;

public class DuplicateAppointmentException : Exception
{
    public DuplicateAppointmentException(string patientName, DateTime date)
        : base($"El paciente {patientName} ya tiene una cita activa en la fecha {date:dd/MM/yyyy}.") { }
}