using FluentValidation;

namespace Piedrazul.Application.Modules.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage("El id de la cita es obligatorio.");

        RuleFor(x => x.NewDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("No se puede re-agendar una cita en una fecha pasada.");

        RuleFor(x => x.NewTime)
            .NotEqual(TimeSpan.Zero).WithMessage("La nueva hora es obligatoria.")
            .LessThan(TimeSpan.FromHours(24)).WithMessage("La hora debe estar entre 00:01 y 23:59.");
    }
}
