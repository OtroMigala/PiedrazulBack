using FluentValidation;

namespace Piedrazul.Application.Modules.Appointments.Queries.GetAppointmentsByDoctorAndDate;

public class GetAppointmentsByDoctorAndDateValidator
    : AbstractValidator<GetAppointmentsByDoctorAndDateQuery>
{
    public GetAppointmentsByDoctorAndDateValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("El médico es obligatorio.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha es obligatoria.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddYears(-1))
            .WithMessage("La fecha no puede ser demasiado antigua.");
    }
}