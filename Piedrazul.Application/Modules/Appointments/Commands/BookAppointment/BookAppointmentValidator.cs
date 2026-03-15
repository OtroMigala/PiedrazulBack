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

        // CA3.5 — Anti-bot: el token debe estar presente (mock funcional)
        // En producción: verificar el token contra reCAPTCHA v3 / hCaptcha API
        RuleFor(x => x.CaptchaToken)
            .NotEmpty().WithMessage("La verificación anti-bot es obligatoria.");
    }
}
