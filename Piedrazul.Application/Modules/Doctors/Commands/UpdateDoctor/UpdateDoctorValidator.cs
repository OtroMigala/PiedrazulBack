using FluentValidation;

namespace Piedrazul.Application.Modules.Doctors.Commands.UpdateDoctor;

public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("El ID del médico es obligatorio.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre del profesional es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar 150 caracteres.");

        RuleFor(x => x.AppointmentIntervalMinutes)
            .GreaterThan(0).WithMessage("El intervalo entre citas debe ser mayor a 0 minutos.");
    }
}
