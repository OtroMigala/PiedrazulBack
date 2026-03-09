using FluentValidation;

namespace Piedrazul.Application.Modules.Appointments.Commands.CreateAppointment;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .MaximumLength(20).WithMessage("El documento no puede superar 20 caracteres.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(150);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El celular es obligatorio.")
            .Matches(@"^[0-9]{7,15}$").WithMessage("El celular debe contener entre 7 y 15 dígitos.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("El médico es obligatorio.");

        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("No se puede agendar una cita en una fecha pasada.");

        RuleFor(x => x.Time)
            .NotEqual(TimeSpan.Zero).WithMessage("La hora de la cita es obligatoria.");
    }
}