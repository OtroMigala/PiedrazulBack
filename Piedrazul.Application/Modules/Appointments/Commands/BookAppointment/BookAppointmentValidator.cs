using FluentValidation;

namespace Piedrazul.Application.Modules.Appointments.Commands.BookAppointment;

public class BookAppointmentValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio.");

        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("El médico es obligatorio.");

        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("No se puede agendar una cita en una fecha pasada.");
    }
}
